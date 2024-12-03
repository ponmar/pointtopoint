namespace PointToPoint.MessageRouting.Factories
{
    public class EventMessageRouterFactory : IMessageRouterFactory
    {
        public IMessageRouter Create(object clientHandler)
        {
            return new EventMessageRouter();
        }
    }
}
