using PointToPoint.Messenger;
using PointToPoint.Server.ClientHandler;

namespace PointToPoint.Server
{
    public record Client(IClientHandler ClientHandler, IMessenger Messenger, IMessageBroadcaster MessageBroadcaster);
}
