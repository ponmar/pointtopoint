namespace PointToPoint.Server
{
    public interface IAppClientMessageHandler
    {
        void Init(IMessageSender messageSender);
    }
}
