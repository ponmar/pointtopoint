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

        private readonly Action<Action>? executor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="executor">Can be used to route message to the UI thread (in WPF: Application.Current.Dispatcher.Invoke, in Avalonia: Avalonia.Threading.Dispatcher.UIThread.Invoke)</param>
        public EventMessageRouter(Action<Action>? executor = null)
        {
            this.executor = executor;
        }

        public void RouteMessage(object message, IMessenger messenger)
        {
            if (executor is not null)
            {
                executor.Invoke(() => MessageReceived?.Invoke(this, new(message, messenger)));
            }
            else
            {
                MessageReceived?.Invoke(this, new(message, messenger));
            }
        }
    }
}
