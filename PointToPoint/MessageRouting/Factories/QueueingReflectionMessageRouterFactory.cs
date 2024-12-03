using PointToPoint.Server.ClientHandler;

namespace PointToPoint.MessageRouting.Factories
{
    public class QueueingReflectionMessageRouterFactory : IMessageRouterFactory
    {
        public IMessageRouter Create(IClientHandler clientHandler)
        {
            return new QueueingReflectionMessageRouter(clientHandler);
        }
    }
}
