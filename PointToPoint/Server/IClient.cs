using PointToPoint.Messenger;

namespace PointToPoint.Server
{
    public interface IClient
    {
        IMessenger Messenger { get; }
        IMessageBroadcaster MessageBroadcaster { get; }
    }
}
