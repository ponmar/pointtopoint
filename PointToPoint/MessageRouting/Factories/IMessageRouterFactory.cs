using PointToPoint.Server.ClientHandler;

namespace PointToPoint.MessageRouting.Factories
{
    public interface IMessageRouterFactory
    {
        IMessageRouter Create(IClientHandler clientHandler);
    }
}
