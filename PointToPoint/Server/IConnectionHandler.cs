using PointToPoint.Messenger.Tcp;

namespace PointToPoint.Server
{
    public interface IConnectionHandler
    {
        void NewConnection(ISocket socket);
    }
}