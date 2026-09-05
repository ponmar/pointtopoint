using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PointToPoint.Messenger.Tcp
{
    public record SocketOptions(bool NoDelay);

    /// <summary>
    /// Message sending over TCP/IP
    /// </summary>
    public class TcpMessenger : Messenger
    {
        private readonly ISocket socket;

        public static readonly SocketOptions DefaultSocketOptions = new(false);

        /// <summary>
        /// Constructor to be used on the client side of the communication
        /// </summary>
        /// Note that this instance can not be re-used after it has disconnected.
        /// Exception will be thrown for errors.
        public TcpMessenger(string serverHostnameOrAddress, int serverPort, IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, ISocketFactory tcpSocketFactory, SocketOptions socketOptions)
            : this(serverHostnameOrAddress, serverPort, payloadSerializer, messageRouter, tcpSocketFactory, socketOptions, new SystemDnsResolver())
        {
        }

        internal TcpMessenger(string serverHostnameOrAddress, int serverPort, IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, ISocketFactory tcpSocketFactory, SocketOptions socketOptions, IDnsResolver dnsResolver)
            : base(payloadSerializer, messageRouter)
        {
            var hosts = dnsResolver.GetHostEntry(serverHostnameOrAddress);

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
        }

        public override void Stop()
        {
            base.Stop();
            Exception? shutdownException = null;

            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception e)
            {
                shutdownException = e;
            }

            try
            {
                socket.Close();
            }
            finally
            {
                socket.Dispose();
            }

            if (shutdownException is not null)
            {
                throw shutdownException;
            }
        }

        protected override async Task ReceiveBytes(ByteBuffer buffer, CancellationToken cancellationToken)
        {
            var numBytesReceived = await socket.ReceiveAsync(buffer.buffer, buffer.offset, buffer.NumBytesLeft, SocketFlags.None, cancellationToken).ConfigureAwait(false);
            if (numBytesReceived == 0)
            {
                throw new SocketException((int)SocketError.ConnectionReset);
            }

            buffer.offset += numBytesReceived;
        }

        protected override async Task SendBytes(byte[] bytes, CancellationToken cancellationToken)
        {
            int numSentBytes = 0;
            while (numSentBytes < bytes.Length)
            {
                var sentBytes = await socket.SendAsync(bytes, numSentBytes, bytes.Length - numSentBytes, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                if (sentBytes == 0)
                {
                    throw new SocketException((int)SocketError.ConnectionReset);
                }

                numSentBytes += sentBytes;
            }
        }
    }
}
