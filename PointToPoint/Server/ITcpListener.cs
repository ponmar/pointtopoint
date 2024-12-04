using PointToPoint.Messenger.Tcp;

namespace PointToPoint.Server
{
    public interface ITcpListener
    {
        void Start();

        ISocket AcceptSocket();
    }
}
