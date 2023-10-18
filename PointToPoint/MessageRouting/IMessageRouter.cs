using System;

namespace PointToPoint.MessageRouting
{
    public interface IMessageRouter
    {
        public bool RouteMessage(object message, Guid senderId);
    }
}