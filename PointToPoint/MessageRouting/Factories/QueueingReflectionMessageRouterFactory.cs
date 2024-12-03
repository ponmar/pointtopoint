namespace PointToPoint.MessageRouting.Factories
{
    public class QueueingReflectionMessageRouterFactory : IMessageRouterFactory
    {
        public IMessageRouter Create(object clientHandler)
        {
            return new QueueingReflectionMessageRouter(clientHandler);
        }
    }
}
