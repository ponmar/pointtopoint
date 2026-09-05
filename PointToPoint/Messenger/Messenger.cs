using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PointToPoint.Messenger
{
    public abstract class Messenger : IMessenger
    {
        public static readonly TimeSpan DefaultKeepAliveSendInterval = TimeSpan.FromSeconds(1);

        public TimeSpan KeepAliveSendInterval { get; set; } = DefaultKeepAliveSendInterval;

        public event EventHandler<Exception>? Disconnected;

        private readonly IPayloadSerializer payloadSerializer;
        private readonly IMessageRouter messageRouter;

        private Task? receiveTask;
        private Task? sendTask;

        private readonly BlockingCollection<byte[]> sendQueue = new();
        private readonly CancellationTokenSource stopTokenSource = new();

        private volatile bool runLoops = true;
        private bool started = false;

        private readonly ByteBuffer lengthBuffer = new(0);
        private readonly ByteBuffer messageBuffer = new(0);
        private int disconnected;
        private Exception? disconnectException;

        protected Messenger(IPayloadSerializer payloadSerializer, IMessageRouter messageRouter)
        {
            this.payloadSerializer = payloadSerializer;
            this.messageRouter = messageRouter;

            ResetLengthBuffer();
        }

        public void Start()
        {
            if (started)
            {
                throw new InvalidOperationException($"This {GetType()} instance has already been started");
            }

            receiveTask = Task.Run(() => ReceiveLoop(stopTokenSource.Token));
            sendTask = Task.Run(() => SendLoop(stopTokenSource.Token));
            started = true;
        }

        public virtual void Stop()
        {
            runLoops = false;
            stopTokenSource.Cancel();
        }

        public bool IsStopped() => !runLoops &&
            (receiveTask is null || receiveTask.IsCompleted) &&
            (sendTask is null || sendTask.IsCompleted);

        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (runLoops && !cancellationToken.IsCancellationRequested)
                {
                    if (!lengthBuffer.Finished)
                    {
                        await ReceiveMessageLength(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await ReceiveMessage(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested || !runLoops)
            {
            }
            catch (Exception e)
            {
                DisconnectAndReportError(e);
            }
        }

        private async Task ReceiveMessageLength(CancellationToken cancellationToken)
        {
            await ReceiveBytes(lengthBuffer, cancellationToken).ConfigureAwait(false);
            if (lengthBuffer.Finished)
            {
                var messageLength = Utils.DeserializeInt(lengthBuffer.buffer);
                ResetMessageBuffer(messageLength);
            }
        }

        private async Task ReceiveMessage(CancellationToken cancellationToken)
        {
            await ReceiveBytes(messageBuffer, cancellationToken).ConfigureAwait(false);
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

        private void DisconnectAndReportError(Exception e)
        {
            // Send and receive loops can fail concurrently; keep only the first exception and
            // ensure shutdown/event notification runs exactly once.
            Interlocked.CompareExchange(ref disconnectException, e, null);
            if (Interlocked.Exchange(ref disconnected, 1) != 0)
            {
                return;
            }

            runLoops = false;
            stopTokenSource.Cancel();
            Disconnected?.Invoke(this, disconnectException ?? e);
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

        private async Task SendLoop(CancellationToken cancellationToken)
        {
            Send(new KeepAlive());
            var keepAliveSentAt = DateTime.UtcNow;

            try
            {
                while (runLoops && !cancellationToken.IsCancellationRequested)
                {
                    var waitUntilKeepAliveMs = MillisecondsUntilKeepAlive(keepAliveSentAt, DateTime.UtcNow);
                    if (sendQueue.TryTake(out var bytes, waitUntilKeepAliveMs, cancellationToken))
                    {
                        await SendBytes(bytes, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    keepAliveSentAt = DateTime.UtcNow;
                    Send(new KeepAlive());
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested || !runLoops)
            {
            }
            catch (Exception e)
            {
                DisconnectAndReportError(e);
            }
        }

        private int MillisecondsUntilKeepAlive(DateTime keepAliveSentAt, DateTime now)
        {
            var remaining = KeepAliveSendInterval - (now - keepAliveSentAt);
            if (remaining <= TimeSpan.Zero)
            {
                return 0;
            }

            return (int)Math.Ceiling(remaining.TotalMilliseconds);
        }

        private void ResetLengthBuffer()
        {
            lengthBuffer.SetTarget(4);
        }

        private void ResetMessageBuffer(int messageLength)
        {
            messageBuffer.SetTarget(messageLength);
        }

        protected abstract Task ReceiveBytes(ByteBuffer buffer, CancellationToken cancellationToken);
        protected abstract Task SendBytes(byte[] bytes, CancellationToken cancellationToken);

        public void Update()
        {
            messageRouter.Update();
        }
    }
}
