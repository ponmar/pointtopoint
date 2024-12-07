using PointToPoint.Messenger.Tcp;

namespace PointToPoint.Server.TcpListener
{
    public class TcpListenerWrapper : ITcpListener
    {
        private readonly System.Net.Sockets.TcpListener tcpListener;

        public TcpListenerWrapper(System.Net.Sockets.TcpListener tcpListener)
        {
            this.tcpListener = tcpListener;
        }

        public void Start() => tcpListener.Start();
        public void Stop() => tcpListener.Stop();
        public ISocket AcceptSocket() => new SocketWrapper(tcpListener.AcceptSocket());
    }
}
