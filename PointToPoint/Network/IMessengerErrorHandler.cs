using System;

namespace PointToPoint.Network
{
    public interface IMessengerErrorHandler
    {
        void PayloadException(Exception e, Guid messengerId);
        void NonProtocolMessageReceived(object message, Guid messengerId);
        void MessageRoutingException(Exception e, Guid messengerId);
        void Disconnected(Guid messengerId);
    }
}
