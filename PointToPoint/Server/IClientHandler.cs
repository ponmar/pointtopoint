using PointToPoint.Messenger.Tcp;

namespace PointToPoint.Server
{
    public interface IClientHandler
    {
        void NewConnection(ISocket socket);
    }
}