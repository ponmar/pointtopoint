using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PointToPoint.Messenger.ErrorHandler
{
    public abstract class AbstractMessenger : IMessenger
    {
        private readonly TimeSpan KeepAliveSendInterval = TimeSpan.FromSeconds(1);

        public Guid Id { get; } = Guid.NewGuid();

        protected readonly IPayloadSerializer payloadSerializer;
        protected readonly string messagesNamespace;
        protected readonly IMessageRouter messageRouter;
        protected readonly IMessengerErrorReporter messengerErrorHandler;

        private readonly Thread receiveThread;
        private readonly Thread sendThread;

        private readonly BlockingCollection<byte[]> sendQueue = new();

        private volatile bool runThreads = true;
        private bool started = false;

        private readonly ByteBuffer lengthBuffer = new(0);
        private readonly ByteBuffer messageBuffer = new(0);

        protected AbstractMessenger(IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, IMessengerErrorReporter messengerErrorHandler)
        {
            this.payloadSerializer = payloadSerializer;
            this.messageRouter = messageRouter;
            this.messengerErrorHandler = messengerErrorHandler;

            ResetLengthBuffer();

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
                catch
                {
                    // Socket receive timeout
                }
            }
        }

        private void ReceiveMessageLength()
        {
            ReceiveBytes(lengthBuffer);
            if (lengthBuffer.Finished)
            {
                var messageLength = Utils.DeserializeInt(lengthBuffer.buffer, 0);
                ResetMessageBuffer(messageLength);
            }
        }

        private void ReceiveMessage()
        {
            ReceiveBytes(messageBuffer);
            if (messageBuffer.Finished)
            {
                // Prepare for next message
                ResetLengthBuffer();

                try
                {
                    var message = payloadSerializer.PayloadToMessage(messageBuffer.buffer, messageBuffer.numBytesToRead);
                    messageRouter.RouteMessage(message, Id);
                }
                catch (Exception e)
                {
                    DisconnectAndReportError(e);
                    return;
                }
            }
        }

        private void DisconnectAndReportError(Exception e = null)
        {
            runThreads = false;
            messengerErrorHandler.Disconnected(Id, e);
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
            catch
            {
                // Socket write error (disconnected)
                // Make receive thread stop
            }

            DisconnectAndReportError();
        }

        private void ResetLengthBuffer()
        {
            lengthBuffer.SetTarget(4);
        }

        private void ResetMessageBuffer(int messageLength)
        {
            messageBuffer.SetTarget(messageLength);
        }

        protected abstract void ReceiveBytes(ByteBuffer buffer);
        protected abstract void SendBytes(byte[] bytes);
    }
}
