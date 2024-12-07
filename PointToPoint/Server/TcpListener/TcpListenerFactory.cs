using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Server.TcpListener
{
    public class TcpListenerFactory : ITcpListenerFactory
    {
        public ITcpListener Create(IPAddress host, int port)
        {
            return new TcpListenerWrapper(new(host, port));
        }
    }
}
