using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PointToPoint.Messenger
{
    public abstract class Messenger : IMessenger
    {
        public static readonly TimeSpan DefaultKeepAliveSendInterval = TimeSpan.FromSeconds(1);

        public TimeSpan KeepAliveSendInterval { get; set; } = DefaultKeepAliveSendInterval;

        public event EventHandler<Exception?>? Disconnected;

        private readonly IPayloadSerializer payloadSerializer;
        private readonly IMessageRouter messageRouter;

        private readonly Thread receiveThread;
        private readonly Thread sendThread;

        private readonly BlockingCollection<byte[]> sendQueue = new();

        private volatile bool runThreads = true;
        private bool started = false;

        private readonly ByteBuffer lengthBuffer = new(0);
        private readonly ByteBuffer messageBuffer = new(0);

        protected Messenger(IPayloadSerializer payloadSerializer, IMessageRouter messageRouter)
        {
            this.payloadSerializer = payloadSerializer;
            this.messageRouter = messageRouter;

            ResetLengthBuffer();

            receiveThread = new Thread(ReceiveThread);
            sendThread = new Thread(SendThread);
        }

        public void Start()
        {
            if (started)
            {
                throw new InvalidOperationException($"This {GetType()} instance has already been started");
            }

            receiveThread.Start();
            sendThread.Start();
            started = true;
        }

        public virtual void Stop()
        {
            runThreads = false;
        }

        public bool IsStopped() => !runThreads && !receiveThread.IsAlive && !sendThread.IsAlive;

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
                var messageLength = Utils.DeserializeInt(lengthBuffer.buffer);
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
                    messageRouter.RouteMessage(message, this);
                }
                catch (Exception e)
                {
                    DisconnectAndReportError(e);
                }
            }
        }

        private void DisconnectAndReportError(Exception? e = null)
        {
            runThreads = false;
            Disconnected?.Invoke(this, e);
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
            var keepAliveSentAt = DateTime.MinValue;

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
                        keepAliveSentAt = now;
                        Send(new KeepAlive());
                    }
                }
            }
            catch
            {
                // Socket write error (disconnected)
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

        public void Update()
        {
            messageRouter.Update();
        }
    }
}
