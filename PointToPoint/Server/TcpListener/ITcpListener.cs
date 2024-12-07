using PointToPoint.Messenger.Tcp;

namespace PointToPoint.Server.TcpListener
{
    public interface ITcpListener
    {
        void Start();
        void Stop();
        ISocket AcceptSocket();
    }
}
