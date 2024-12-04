using System.Net;

namespace PointToPoint.Server
{
    public interface ITcpListenerFactory
    {
        ITcpListener Create(IPAddress host, int port);
    }
}
