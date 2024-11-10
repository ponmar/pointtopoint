using PointToPoint.Server.ClientHandler;

namespace PointToPoint.Server
{
    public interface IMessageSender
    {
        void SendMessage(object message, IClientHandler sender);
        void SendBroadcast(object message);
    }
}