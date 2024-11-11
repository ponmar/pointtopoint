using PointToPoint.Server.ClientHandler;

namespace PointToPoint.MessageRouting.Factories
{
    public class EventMessageRouterFactory : IMessageRouterFactory
    {
        public IMessageRouter Create(IClientHandler clientHandler)
        {
            return new EventMessageRouter();
        }
    }
}
