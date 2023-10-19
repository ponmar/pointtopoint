﻿using PointToPoint.MessageRouting;
using PointToPoint.Messenger.ErrorHandler;
using PointToPoint.Payload;
using System.Linq;
using System.Net;
//using System.Buffers.Binary;
using System.Net.Sockets;

namespace PointToPoint.Messenger
{
    // Message sending over TCP/IP
    public class TcpMessenger : AbstractMessenger
    {
        private readonly Socket socket;

        // Client side
        public TcpMessenger(string serverHostname, int serverPort, IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, IMessengerErrorHandler messengerErrorHandler)
            : base(payloadSerializer, messageRouter, messengerErrorHandler)
        {
            var servers = Dns.GetHostEntry(serverHostname);
            var server = servers.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            socket = new(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SetSocketOptions();
            socket.Connect(new IPEndPoint(new IPAddress(server.GetAddressBytes()), serverPort));
        }

        // Server side (when server socket accepted new client socket)
        internal TcpMessenger(Socket socket, IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, IMessengerErrorHandler messengerErrorHandler)
            : base(payloadSerializer, messageRouter, messengerErrorHandler)
        {
            this.socket = socket;
            SetSocketOptions();
        }

        private void SetSocketOptions()
        {
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
                //var span = new Span<byte>(bytes, numSentBytes, bytes.Length - numSentBytes);
                //numSentBytes += socket.Send(span);
                numSentBytes += socket.Send(bytes, numSentBytes, bytes.Length - numSentBytes, SocketFlags.None);
            }
        }
    }
}
