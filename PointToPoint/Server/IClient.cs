using PointToPoint.Messenger;
using System;

namespace PointToPoint.Server
{
    public interface IClient
    {
        IMessenger Messenger { get; }
        IMessageBroadcaster MessageBroadcaster { get; }
        void Disconnect(Exception? e = null);
    }
}
