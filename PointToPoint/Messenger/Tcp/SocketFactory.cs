using System.Net.Sockets;

namespace PointToPoint.Messenger.Tcp
{
    public interface ISocketFactory
    {
        ISocket Create(AddressFamily addressFamily);
    }

    /// <summary>
    /// Creates a real socket.
    /// </summary>
    public class SocketFactory : ISocketFactory
    {
        public ISocket Create(AddressFamily addressFamily)
        {
            return new SocketWrapper(new(addressFamily, SocketType.Stream, ProtocolType.Tcp));
        }
    }
}
