using PointToPoint.Messenger.Tcp;

namespace PointToPoint.Server
{
    public interface ITcpListener
    {
        void Start();
        void Stop();
        ISocket AcceptSocket();
    }
}
