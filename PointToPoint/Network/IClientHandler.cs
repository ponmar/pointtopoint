using System.Net.Sockets;

namespace PointToPoint.Network
{
    public interface IClientHandler
    {
        void NewConnection(Socket socket);
    }
}