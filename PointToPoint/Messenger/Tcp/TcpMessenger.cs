using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Messenger.Tcp
{
    /// <summary>
    /// Message sending over TCP/IP 
    /// </summary>
    public class TcpMessenger : Messenger
    {
        private readonly ISocket socket;

        /// <summary>
        /// Constructor to be used on the client side of the communication
        /// </summary>
        /// Note that this instance can not be re-used after it has disconnected.
        /// Exception will be thrown for errors.
        public TcpMessenger(string serverHostname, int serverPort, IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, ISocketFactory tcpSocketFactory, TimeSpan keepAliveSendInterval)
            : base(payloadSerializer, messageRouter, keepAliveSendInterval)
        {
            var servers = Dns.GetHostEntry(serverHostname);
            var server = servers.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            socket = tcpSocketFactory.Create(server.AddressFamily);
            SetSocketOptions();
            socket.Connect(new IPEndPoint(new IPAddress(server.GetAddressBytes()), serverPort));
        }

        /// <summary>
        /// Constructor to be used internally on the server side (when server socket accepted new client socket)
        /// </summary>
        internal TcpMessenger(ISocket socket, IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, TimeSpan keepAliveSendInterval)
            : base(payloadSerializer, messageRouter, keepAliveSendInterval)
        {
            this.socket = socket;
            SetSocketOptions();
        }

        private void SetSocketOptions()
        {
            socket.NoDelay = true;

            // Note that this setting affects how fast the receive thread can be shut down.
            socket.ReceiveTimeout = 500;
        }

        public override void Stop()
        {
            base.Stop();
            if (socket.Connected)
            {
                socket.Close();
            }
            socket.Dispose();
        }

        protected override void ReceiveBytes(ByteBuffer buffer)
        {
            var numBytesReceived = socket.Receive(buffer.buffer, buffer.offset, buffer.NumBytesLeft, SocketFlags.None);
            buffer.offset += numBytesReceived;
        }

        protected override void SendBytes(byte[] bytes)
        {
            int numSentBytes = 0;
            while (numSentBytes < bytes.Length)
            {
                numSentBytes += socket.Send(bytes, numSentBytes, bytes.Length - numSentBytes, SocketFlags.None);
            }
        }
    }
}
