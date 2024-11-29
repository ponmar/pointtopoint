using PointToPoint.Messenger;
using System;

namespace PointToPoint.MessageRouting
{
    /// <summary>
    /// Handles custom forwarding of messages via a lambda expression.
    /// </summary>
    public class CustomMessageRouter : IMessageRouter
    {
        private readonly Action<object, IMessenger> routeAction;

        private readonly Action<Action>? executor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="routeAction">object is the message to be routed</param>
        /// <param name="executor">Can be used to route message on the UI thread in a WPF application by setting executor:Application.Current.Dispatcher.Invoke</param>
        public CustomMessageRouter(Action<object, IMessenger> routeAction, Action<Action>? executor = null)
        {
            this.routeAction = routeAction;
            this.executor = executor;
        }

        public void RouteMessage(object message, IMessenger messenger)
        {
            if (executor is not null)
            {
                executor.Invoke(() => routeAction(message, messenger));
            }
            else
            {
                routeAction(message, messenger);
            }
        }
    }
}
