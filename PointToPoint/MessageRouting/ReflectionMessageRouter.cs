using PointToPoint.Messenger;
using System;

namespace PointToPoint.MessageRouting
{
    /// <summary>
    /// Handles forwarding of messages to the specified handler class.
    /// </summary>
    /// The handler class needs to implement one method per handled message according to:
    ///
    /// void HandleMessage(MyMessage message, IMessenger messenger)
    /// {
    ///     // Message handling code
    /// }
    /// 
    /// Note that the handle method is called from the internal message receiver thread when no executor is specified.
    public class ReflectionMessageRouter : IMessageRouter
    {
        private const string handleMethodName = "HandleMessage";

        private readonly object messageHandler;
        private readonly Action<Action>? executor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageHandler">Instance that implements the messeage handling methods</param>
        /// <param name="executor">Can be used to route message on the UI thread in a WPF application by setting executor:Application.Current.Dispatcher.Invoke</param>
        public ReflectionMessageRouter(object messageHandler, Action<Action>? executor = null)
        {
            this.messageHandler = messageHandler;
            this.executor = executor;
        }

        public void RouteMessage(object message, IMessenger messenger)
        {
            var argTypes = new Type[] { message.GetType(), typeof(IMessenger) };

            var handleMethod = messageHandler.GetType().GetMethod(handleMethodName, argTypes);
            if (handleMethod is null)
            {
                throw new NotImplementedException($"Message handling method ({handleMethodName}) not implemented for message type {message.GetType()}");
            }

            var args = new object[] { message, messenger };
            if (executor is not null)
            {
                executor.Invoke(() => handleMethod.Invoke(messageHandler, args));
            }
            else
            {
                handleMethod.Invoke(messageHandler, args);
            }
        }
    }
}
