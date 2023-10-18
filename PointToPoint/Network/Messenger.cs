using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace PointToPoint.Network
{
    public record KeepAlive();

    public interface IMessengerErrorHandler
    {
        void PayloadException(Exception e, Guid messengerId);
        void NonProtocolMessageReceived(object message, Guid messengerId);
        void MessageRoutingException(Exception e, Guid messengerId);
        void Disconnected(Guid messengerId);
    }

    public abstract class Messenger : IMessenger
    {
        private readonly TimeSpan KeepAliveSendInterval = TimeSpan.FromSeconds(1);

        public Guid Id { get; } = Guid.NewGuid();

        protected readonly IPayloadSerializer payloadSerializer;
        protected readonly string messagesNamespace;
        protected readonly IMessageRouter messageRouter;
        protected readonly IMessengerErrorHandler messengerErrorHandler;

        private volatile bool runThreads = true;

        private readonly Thread receiveThread;
        private readonly Thread sendThread;

        private readonly BlockingCollection<byte[]> sendQueue = new();
        private bool started = false;

        // TODO: move to own class?
        private byte[] receiveBuffer = new byte[4]; // Size is adapted for largest received message
        private int receiveBufferOffset;
        private int receivedMessageLength;

        protected Messenger(IPayloadSerializer payloadSerializer, string messagesNamespace, IMessageRouter messageRouter, IMessengerErrorHandler messengerErrorHandler)
        {
            this.payloadSerializer = payloadSerializer;
            this.messagesNamespace = messagesNamespace;
            this.messageRouter = messageRouter;
            this.messengerErrorHandler = messengerErrorHandler;

            receiveThread = new Thread(ReceiveThread);
            sendThread = new Thread(SendThread);

            ResetReceiveCounters();
        }

        public void Start()
        {
            if (started)
            {
                throw new InvalidOperationException("This messenger has already been started");
            }

            receiveThread.Start();
            sendThread.Start();
            started = true;
        }

        public virtual void Close()
        {
            runThreads = false;
            sendThread.Join();
            receiveThread.Join();
        }

        private void ReceiveThread(object _)
        {
            while (runThreads)
            {
                try
                {
                    if (!MessageLengthReceived)
                    {
                        ReceiveMessageLength();
                    }
                    else
                    {
                        ReceiveMessage();
                    }
                }
                catch (SocketException)
                {
                    // Socket receive timeout
                }
            }
            messengerErrorHandler.Disconnected(Id);
        }

        private bool MessageLengthReceived => receivedMessageLength != -1;

        private void ReceiveMessageLength()
        {
            var numBytesReceived = ReceiveBytes(receiveBuffer, 0, 4 - receiveBufferOffset);
            if (numBytesReceived > 0)
            {
                receiveBufferOffset += numBytesReceived;
                if (receiveBufferOffset == 4)
                {
                    receivedMessageLength = DeserializeInt(receiveBuffer, 0);

                    if (receiveBuffer.Length < 4 + receivedMessageLength)
                    {
                        // Make message fit in buffer if needed
                        receiveBuffer = new byte[4 + receivedMessageLength];
                    }
                }
            }
        }

        abstract protected int ReceiveBytes(byte[] buffer, int bufferOffset, int size);

        private void ReceiveMessage()
        {
            int payloadBytesLeft = receivedMessageLength - receiveBufferOffset + 4;
            var numBytesReceived = ReceiveBytes(receiveBuffer, receiveBufferOffset, payloadBytesLeft);
            if (numBytesReceived > 0)
            {
                receiveBufferOffset += numBytesReceived;
                if (receiveBufferOffset == receivedMessageLength + 4)
                {
                    object message;
                    try
                    {
                        message = payloadSerializer.PayloadToMessage(receiveBuffer, 4, receivedMessageLength);
                        ResetReceiveCounters();
                    }
                    catch (Exception e)
                    {
                        messengerErrorHandler.PayloadException(e, Id);
                        ResetReceiveCounters();
                        return;
                    }

                    // TODO: move namespace checks to PayloadToMessage to make the check before parsing json data?
                    
                    if (message.GetType() == typeof(KeepAlive))
                    {
                        Console.WriteLine($"Received {nameof(KeepAlive)}");
                        return;
                    }

                    if (message.GetType().Namespace != messagesNamespace)
                    {
                        messengerErrorHandler.NonProtocolMessageReceived(message, Id);
                        return;
                    }

                    try
                    {
                        messageRouter.RouteMessage(message, Id);
                    }
                    catch (Exception e)
                    {
                        messengerErrorHandler.MessageRoutingException(e, Id);
                        return;
                    }
                }
            }
        }

        private void ResetReceiveCounters()
        {
            receivedMessageLength = -1;
            receiveBufferOffset = 0;
        }

        public void Send(object message)
        {
            var payloadBytes = payloadSerializer.MessageToPayload(message);
            var lengthBytes = SerializeInt(payloadBytes.Length);

            var bytes = new byte[4 + payloadBytes.Length];
            lengthBytes.CopyTo(bytes, 0);
            payloadBytes.CopyTo(bytes, 4);

            sendQueue.Add(bytes);
        }

        private void SendThread(object _)
        {
            var keepAliveSentAt = DateTime.Now;

            try
            {
                while (runThreads)
                {
                    if (sendQueue.TryTake(out var bytes, 1000))
                    {
                        SendBytes(bytes);
                    }

                    var now = DateTime.Now;
                    if (now - keepAliveSentAt > KeepAliveSendInterval)
                    {
                        Console.WriteLine($"Sending {nameof(KeepAlive)}");
                        keepAliveSentAt = now;
                        Send(new KeepAlive());
                    }
                }
            }
            catch (SocketException)
            {
                // Socket write error (disconnected)
            }
            messengerErrorHandler.Disconnected(Id);
        }

        protected abstract void SendBytes(byte[] bytes);


        private static byte[] SerializeInt(int value)
        {
            /*
            var bytes = new byte[4];
            var span = new Span<byte>(bytes);
            BinaryPrimitives.WriteInt32BigEndian(span, value);
            return bytes;
            */

            return BitConverter.GetBytes(value);
        }

        private static int DeserializeInt(byte[] bytes, int offset)
        {
            /*
            var span = new Span<byte>(bytes, offset, 4);
            return BinaryPrimitives.ReadInt32BigEndian(span);
            */
            return BitConverter.ToInt32(bytes, offset);
        }
    }
}
