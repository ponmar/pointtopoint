using System;

namespace PointToPoint.MessageRouting
{
    public interface IMessageRouter
    {
        public void RouteMessage(object message, Guid senderId);
    }
}