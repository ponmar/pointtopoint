using PointToPoint.MessageRouting;
using PointToPoint.Payload;
using System.Linq;
using System.Net;
//using System.Buffers.Binary;
using System.Net.Sockets;

namespace PointToPoint.Network
{
    // Message sending over TCP/IP
    public class TcpMessenger : Messenger
    {
        private readonly Socket socket;

        // Client side
        public TcpMessenger(string serverHostname, int serverPort, IPayloadSerializer payloadSerializer, string messagesNamespace, IMessageRouter messageRouter, IMessengerErrorHandler messengerErrorHandler)
            : base(payloadSerializer, messagesNamespace, messageRouter, messengerErrorHandler)
        {
            var servers = Dns.GetHostEntry(serverHostname);
            var server = servers.AddressList.First();
            socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                ReceiveTimeout = 500
            };
            socket.Connect(new IPEndPoint(new IPAddress(server.GetAddressBytes()), serverPort));
        }

        // Server side
        public TcpMessenger(Socket socket, IPayloadSerializer payloadSerializer, string messagesNamespace, IMessageRouter messageRouter, IMessengerErrorHandler messengerErrorHandler)
            : base(payloadSerializer, messagesNamespace, messageRouter, messengerErrorHandler)
        {
            this.socket = socket;
            socket.NoDelay = true;
            socket.ReceiveTimeout = 500;
        }

        public override void Close()
        {
            base.Close();
            if (socket.Connected)
            {
                socket.Close();
            }
            socket.Dispose();
        }

        protected override int ReceiveBytes(byte[] buffer, int bufferOffset, int size)
        {
            return socket.Receive(buffer, bufferOffset, size, SocketFlags.None);
        }

        protected override void SendBytes(byte[] bytes)
        {
            int numSentBytes = 0;
            while (numSentBytes < bytes.Length)
            {
                //var span = new Span<byte>(bytes, numSentBytes, bytes.Length - numSentBytes);
                //numSentBytes += socket.Send(span);
                numSentBytes += socket.Send(bytes, numSentBytes, bytes.Length - numSentBytes, SocketFlags.None);

                if (numSentBytes != bytes.Length)
                {
                    //log.Warn("Complete message not sent in one go");
                }
            }
        }
    }
}
