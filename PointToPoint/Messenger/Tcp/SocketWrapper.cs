using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
        public bool Connected => socket.Connected;

        public void Shutdown(SocketShutdown how) => socket.Shutdown(how);

        public void Close() => socket.Close();

        public void Connect(EndPoint remoteEP) => socket.Connect(remoteEP);

        public void Dispose() => socket.Dispose();

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) => socket.Receive(buffer, offset, size, socketFlags);

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags) => socket.Send(buffer, offset, size, socketFlags);

        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            return socket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, size), socketFlags);
        }

        public Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            return socket.SendAsync(new ArraySegment<byte>(buffer, offset, size), socketFlags);
        }
    }
}
