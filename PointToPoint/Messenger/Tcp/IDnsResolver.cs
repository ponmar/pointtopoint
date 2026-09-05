using System.Net;

namespace PointToPoint.Messenger.Tcp
{
    public interface IDnsResolver
    {
        IPHostEntry GetHostEntry(string hostNameOrAddress);
    }

    internal sealed class SystemDnsResolver : IDnsResolver
    {
        public IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            return Dns.GetHostEntry(hostNameOrAddress);
        }
    }
}
