using System.Net.Sockets;

namespace PointToPoint.Server
{
    public interface IClientHandler
    {
        void NewConnection(Socket socket);
    }
}