using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Messenger.Tcp
{
    internal class SocketWrapper : ISocket
    {
        private readonly Socket socket;

        public SocketWrapper(Socket socket)
        {
            this.socket = socket;
        }

        public bool NoDelay { set => socket.NoDelay = value; }
        public int ReceiveTimeout { set => socket.ReceiveTimeout = value; }

        public bool Connected => socket.Connected;

        public void Close() => socket.Close();

        public void Connect(EndPoint remoteEP) => socket.Connect(remoteEP);

        public void Dispose() => socket.Dispose();

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) => socket.Receive(buffer, offset, size, socketFlags);

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags) => socket.Send(buffer, offset, size, socketFlags);
    }
}
