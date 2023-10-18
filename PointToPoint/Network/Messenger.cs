using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace PointToPoint.Network
{
    public abstract class Messenger : IMessenger
    {
        private readonly TimeSpan KeepAliveSendInterval = TimeSpan.FromSeconds(1);

        public Guid Id { get; } = Guid.NewGuid();

        protected readonly IPayloadSerializer payloadSerializer;
        protected readonly string messagesNamespace;
        protected readonly IMessageRouter messageRouter;
        protected readonly IMessengerErrorHandler messengerErrorHandler;

        private readonly Thread receiveThread;
        private readonly Thread sendThread;

        private readonly BlockingCollection<byte[]> sendQueue = new();

        // TODO: replace bool flags with a state enum?
        private volatile bool runThreads = true;
        private bool started = false;

        private readonly MessageByteBuffer messageLengthBuffer = new(4);
        private readonly MessageByteBuffer messageBuffer = new(4);

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
                    if (!messageLengthBuffer.Finished)
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
            var numBytesReceived = ReceiveBytes(messageLengthBuffer.buffer, 0, 4 - messageLengthBuffer.offset);
            if (numBytesReceived > 0)
            {
                messageLengthBuffer.offset += numBytesReceived;
                if (messageLengthBuffer.Finished)
                {
                    var messageLength = DeserializeInt(messageLengthBuffer.buffer, 0);
                    messageBuffer.SetTarget(messageLength);
                }
            }
        }

        abstract protected int ReceiveBytes(byte[] buffer, int bufferOffset, int size);

        private void ReceiveMessage()
        {
            var numBytesReceived = ReceiveBytes(messageBuffer.buffer, messageBuffer.offset, messageBuffer.NumBytesLeft);
            if (numBytesReceived > 0)
            {
                messageBuffer.offset += numBytesReceived;
                if (messageBuffer.Finished)
                {
                    object message;
                    try
                    {
                        message = payloadSerializer.PayloadToMessage(messageBuffer.buffer, messageBuffer.numBytesToRead);
                        messageLengthBuffer.SetTarget(4);
                    }
                    catch (Exception e)
                    {
                        messengerErrorHandler.PayloadException(e, Id);
                        messageLengthBuffer.SetTarget(4);
                        return;
                    }
                                        
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
