using System.Collections.Generic;
using System.Net.Sockets;

namespace PointToPoint.Network
{
    public interface IClientHandler
    {
        List<TcpMessenger> Clients { get; }

        void NewConnection(Socket socket);
    }
}