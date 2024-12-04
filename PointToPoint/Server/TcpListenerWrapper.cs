using PointToPoint.Messenger.Tcp;
using System.Net.Sockets;

namespace PointToPoint.Server
{
    public class TcpListenerWrapper : ITcpListener
    {
        private readonly TcpListener tcpListener;

        public TcpListenerWrapper(TcpListener tcpListener)
        {
            this.tcpListener = tcpListener;
        }

        public void Start() => tcpListener.Start();
        public void Stop() => tcpListener.Stop();
        public ISocket AcceptSocket() => new SocketWrapper(tcpListener.AcceptSocket());
    }
}
