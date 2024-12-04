using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Server
{
    public class TcpListenerFactory : ITcpListenerFactory
    {
        public ITcpListener Create(IPAddress host, int port)
        {
            return new TcpListenerWrapper(new TcpListener(host, port));
        }
    }
}
