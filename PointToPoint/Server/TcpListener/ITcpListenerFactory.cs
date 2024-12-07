using System.Net;

namespace PointToPoint.Server.TcpListener
{
    public interface ITcpListenerFactory
    {
        ITcpListener Create(IPAddress host, int port);
    }
}
