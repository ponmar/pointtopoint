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

        private volatile bool runThreads = true;
        private bool started = false;

        private readonly ByteBuffer lengthBuffer = new(4);
        private readonly ByteBuffer messageBuffer = new(0);

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
                    if (!lengthBuffer.Finished)
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
            ReceiveBytes(lengthBuffer);
            if (lengthBuffer.Finished)
            {
                var messageLength = Utils.DeserializeInt(lengthBuffer.buffer, 0);
                messageBuffer.SetTarget(messageLength);
            }
        }

        private void ReceiveMessage()
        {
            ReceiveBytes(messageBuffer);
            if (messageBuffer.Finished)
            {
                object message;
                try
                {
                    message = payloadSerializer.PayloadToMessage(messageBuffer.buffer, messageBuffer.numBytesToRead);
                    lengthBuffer.SetTarget(4);
                }
                catch (Exception e)
                {
                    messengerErrorHandler.PayloadException(e, Id);
                    lengthBuffer.SetTarget(4);
                    return;
                }

                if (message.GetType() != typeof(KeepAlive) &&
                    message.GetType().Namespace != messagesNamespace)
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

        public void Send(object message)
        {
            var payloadBytes = payloadSerializer.MessageToPayload(message);
            var lengthBytes = Utils.SerializeInt(payloadBytes.Length);

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

        protected abstract void ReceiveBytes(ByteBuffer buffer);
        protected abstract void SendBytes(byte[] bytes);
    }
}
