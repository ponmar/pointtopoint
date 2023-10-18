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

    public class MessageByteBuffer
    {
        public byte[] buffer = new byte[4];
        public int offset;

        public int MessageLengthBytesLeft => Math.Max(4 - offset, 0);
        public int MessageLength { get; private set; }

        public int MessageBytesLeft => Math.Max(MessageLength - offset + 4, 0);

        public MessageByteBuffer()
        {
            Reset();
        }

        public void Reset()
        {
            offset = 0;
            MessageLength = 0;
        }

        public void SetMessageLength(int messageLength)
        {
            MessageLength = messageLength;
            if (buffer.Length < 4 + messageLength)
            {
                // Make message fit in buffer if needed
                buffer = new byte[4 + messageLength];
            }
        }
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

        private readonly MessageByteBuffer receiveBuffer = new();

        protected Messenger(IPayloadSerializer payloadSerializer, string messagesNamespace, IMessageRouter messageRouter, IMessengerErrorHandler messengerErrorHandler)
        {
            this.payloadSerializer = payloadSerializer;
            this.messagesNamespace = messagesNamespace;
            this.messageRouter = messageRouter;
            this.messengerErrorHandler = messengerErrorHandler;

            receiveThread = new Thread(ReceiveThread);
            sendThread = new Thread(SendThread);
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
                    if (receiveBuffer.MessageLengthBytesLeft > 0)
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


        private void ReceiveMessageLength()
        {
            var numBytesReceived = ReceiveBytes(receiveBuffer.buffer, 0, 4 - receiveBuffer.offset);
            if (numBytesReceived > 0)
            {
                receiveBuffer.offset += numBytesReceived;
                if (receiveBuffer.offset == 4)
                {
                    var messageLength = DeserializeInt(receiveBuffer.buffer, 0);
                    receiveBuffer.SetMessageLength(messageLength);
                }
            }
        }

        abstract protected int ReceiveBytes(byte[] buffer, int bufferOffset, int size);

        private void ReceiveMessage()
        {
            var payloadBytesLeft = receiveBuffer.MessageBytesLeft;
            var numBytesReceived = ReceiveBytes(receiveBuffer.buffer, receiveBuffer.offset, payloadBytesLeft);
            if (numBytesReceived > 0)
            {
                receiveBuffer.offset += numBytesReceived;
                if (receiveBuffer.MessageBytesLeft == 0)
                {
                    object message;
                    try
                    {
                        message = payloadSerializer.PayloadToMessage(receiveBuffer.buffer, 4, receiveBuffer.MessageLength);
                        receiveBuffer.Reset();
                    }
                    catch (Exception e)
                    {
                        messengerErrorHandler.PayloadException(e, Id);
                        receiveBuffer.Reset();
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
