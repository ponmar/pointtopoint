using System.Net.Sockets;

namespace PointToPoint.Network.Server
{
    public interface IClientHandler
    {
        void NewConnection(Socket socket);
    }
}