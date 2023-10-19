using System;

namespace PointToPoint.Messenger.ErrorHandler
{
    /// <summary>
    /// Notifications to the applications about communication problems
    /// </summary>
    public interface IMessengerErrorHandler
    {
        void PayloadException(Exception e, Guid messengerId);
        void MessageRoutingException(Exception e, Guid messengerId);
        void Disconnected(Guid messengerId);
    }
}
