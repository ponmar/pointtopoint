namespace PointToPoint.MessageRouting.Factories
{
    public interface IMessageRouterFactory
    {
        IMessageRouter Create(object clientHandler);
    }
}
