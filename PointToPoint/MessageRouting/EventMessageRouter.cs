using System;

namespace PointToPoint.MessageRouting
{
    public record MessageInfo(object Message, Guid ConnectionId);

    /// <summary>
    /// Handles forwarding of messages to handler class via an event.
    /// </summary>
    public class EventMessageRouter : IMessageRouter
    {
        /// <summary>
        /// Note that this event is fired from the internal message receiving thread
        /// </summary>
        public EventHandler<MessageInfo> MessageReceived;

        public void RouteMessage(object message, Guid senderId)
        {
            MessageReceived?.Invoke(this, new(message, senderId));
        }
    }
}
