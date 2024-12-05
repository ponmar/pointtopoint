namespace PointToPoint.Server
{
    public interface IMessageBroadcaster
    {
        void SendBroadcast(object message);
    }
}