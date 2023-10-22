using PointToPoint.Messenger;
using System;

namespace PointToPoint.MessageRouting
{
    public record MessageInfo(object Message, IMessenger Messenger);

    /// <summary>
    /// Handles forwarding of messages to handler class via an event.
    /// </summary>
    public class EventMessageRouter : IMessageRouter
    {
        /// <summary>
        /// Note that this event is fired from the internal message receiving thread
        /// </summary>
        public EventHandler<MessageInfo>? MessageReceived;

        public void RouteMessage(object message, IMessenger messenger)
        {
            MessageReceived?.Invoke(this, new(message, messenger));
        }
    }
}
