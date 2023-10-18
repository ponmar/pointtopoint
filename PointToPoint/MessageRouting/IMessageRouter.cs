using PointToPoint.Network;

namespace PointToPoint.MessageRouting
{
    public interface IMessageRouter
    {
        public bool RouteMessage(object message);
    }
}