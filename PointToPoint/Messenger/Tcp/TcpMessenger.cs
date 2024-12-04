using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Messenger.Tcp
{
    // Note that ReceiveTimeout affects how fast the receive thread can be shut down.
    public record SocketOptions(bool NoDelay, TimeSpan ReceiveTimeout);

    /// <summary>
    /// Message sending over TCP/IP 
    /// </summary>
    public class TcpMessenger : Messenger
    {
        private readonly ISocket socket;

        public static readonly SocketOptions DefaultSocketOptions = new(false, TimeSpan.FromMilliseconds(500));

        /// <summary>
        /// Constructor to be used on the client side of the communication
        /// </summary>
        /// Note that this instance can not be re-used after it has disconnected.
        /// Exception will be thrown for errors.
        public TcpMessenger(string serverHostnameOrAddress, int serverPort, IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, ISocketFactory tcpSocketFactory, SocketOptions socketOptions)
            : base(payloadSerializer, messageRouter)
        {
            var hosts = Dns.GetHostEntry(serverHostnameOrAddress);

            var servers = hosts.AddressList.Where(x =>
                x.AddressFamily == AddressFamily.InterNetwork ||
                x.AddressFamily == AddressFamily.InterNetworkV6);

            if (!servers.Any())
            {
                throw new Exception($"No such server hostname available: {serverHostnameOrAddress}");
            }

            Exception? lastConnectException = null;

            // Connect with IPv4 first and then IPv6
            foreach (var server in servers.OrderBy(x => x.AddressFamily))
            {
                try
                {
                    socket = tcpSocketFactory.Create(server.AddressFamily);
                    SetSocketOptions(socketOptions);
                    socket.Connect(new IPEndPoint(new IPAddress(server.GetAddressBytes()), serverPort));
                    return;
                }
                catch (Exception e)
                {
                    lastConnectException = e;
                }
            }

            throw new Exception("Unable to connect to any server", lastConnectException);
        }

        /// <summary>
        /// Constructor to be used internally on the server side (when server socket accepted new client socket)
        /// </summary>
        internal TcpMessenger(ISocket socket, IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, SocketOptions socketOptions)
            : base(payloadSerializer, messageRouter)
        {
            this.socket = socket;
            SetSocketOptions(socketOptions);
        }

        private void SetSocketOptions(SocketOptions socketOptions)
        {
            socket.NoDelay = socketOptions.NoDelay;
            socket.ReceiveTimeout = (int)socketOptions.ReceiveTimeout.TotalMilliseconds;
        }

        public override void Stop()
        {
            base.Stop();
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
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
