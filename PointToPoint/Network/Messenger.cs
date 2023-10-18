using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace PointToPoint.Network
{
    // Sends and receives messages over TCP in format: <length (4 bytes)> <payload>
    public abstract class Messenger
    {
        public Guid Id { get; } = Guid.NewGuid();

        protected readonly IPayloadSerializer payloadSerializer;
        protected readonly string messagesNamespace;
        protected readonly IMessageRouter messageRouter;

        private volatile bool runThreads = true;

        private readonly Thread receiveThread;
        private readonly Thread sendThread;

        private readonly BlockingCollection<byte[]> sendQueue = new();
        private bool started = false;

        private byte[] receiveBuffer = new byte[4]; // Size is adapted for largest received message
        private int receiveBufferOffset = 0;
        private int receivedMessageLength = -1;

        protected Messenger(IPayloadSerializer payloadSerializer, string messagesNamespace, IMessageRouter messageRouter)
        {
            this.payloadSerializer = payloadSerializer;
            this.messagesNamespace = messagesNamespace;
            this.messageRouter = messageRouter;

            receiveThread = new Thread(ReceiveThread);
            sendThread = new Thread(SendThread);
        }

        public void StartCommunication()
        {
            if (!started)
            {
                receiveThread.Start();
                sendThread.Start();
                started = true;
            }
        }

        public virtual void Close()
        {
            runThreads = false;
            sendThread.Join();
            receiveThread.Join();
        }

        public bool IsHealthy()
        {
            // TODO: check that not too many messages has been queued? Meaning that client is too slow at receiving messages...
            return !started || (sendThread.IsAlive && receiveThread.IsAlive);
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
        }

        private bool MessageLengthReceived => receivedMessageLength != -1;

        private void ReceiveMessageLength()
        {
            //int numBytesReceived = socket.Receive(receiveBuffer, 0, 4 - receiveBufferOffset, SocketFlags.None);
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
                    try
                    {
                        var message = payloadSerializer.PayloadToMessage(receiveBuffer, 4, receivedMessageLength);
                        var messageType = message.GetType();
                        if (messageType.Namespace == messagesNamespace)
                        {
                            messageRouter.RouteMessage(message);
                        }
                        else
                        {
                            throw new Exception("Received message not part of protocol");
                        }
                    }
                    catch
                    {
                        // TODO: log exception?
                    }

                    receivedMessageLength = -1;
                    receiveBufferOffset = 0;
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
            try
            {
                while (runThreads)
                {
                    if (sendQueue.TryTake(out var bytes, 1000))
                    {
                        SendBytes(bytes);
                    }
                }
            }
            catch (SocketException)
            {
                // Socket write error (disconnected). Stop thread and let supervision detect that the thread is not alive.
            }
        }

        protected abstract void SendBytes(byte[] bytes);


        protected static byte[] SerializeInt(int value)
        {
            /*
            var bytes = new byte[4];
            var span = new Span<byte>(bytes);
            BinaryPrimitives.WriteInt32BigEndian(span, value);
            return bytes;
            */

            return BitConverter.GetBytes(value);
        }

        protected static int DeserializeInt(byte[] bytes, int offset)
        {
            /*
            var span = new Span<byte>(bytes, offset, 4);
            return BinaryPrimitives.ReadInt32BigEndian(span);
            */
            return BitConverter.ToInt32(bytes, offset);
        }
    }
}
