using System;

namespace PointToPoint.Server
{
    /// <summary>
    /// The interface for server side specific data per client connection
    /// </summary>
    /// The Dispose method can be used to perform cleanup when a client is disconnected
    public interface IClientHandler : IDisposable
    {
        void Init(IMessageSender messageSender);
    }
}
