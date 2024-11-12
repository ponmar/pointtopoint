using PointToPoint.Server.ClientHandler;

namespace PointToPoint.MessageRouting.Factories
{
    public class QueuingReflectionMessageRouterFactory : IMessageRouterFactory
    {
        public IMessageRouter Create(IClientHandler clientHandler)
        {
            return new QueuingReflectionMessageRouter(clientHandler);
        }
    }
}
