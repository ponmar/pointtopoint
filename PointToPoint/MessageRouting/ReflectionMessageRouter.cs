using PointToPoint.Messenger;
using System;

namespace PointToPoint.MessageRouting
{
    /// <summary>
    /// Handles forwarding of messages to the specified handler class.
    /// </summary>
    /// The handler class needs to implement one method per handled message according to:
    ///
    /// void HandleMessage(MyMessage message, Guid senderId)
    /// {
    ///     // Message handling code
    /// }
    /// 
    /// Note that the handle method is called from the internal message receiver thread.
    public class ReflectionMessageRouter : IMessageRouter
    {
        private readonly string handleMethodName;
        private readonly Action<Action>? executor;

        public object? MessageHandler { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handleMethodName">Name of the handler method</param>
        /// <param name="executor">Can be used to route message on the UI thread in a WPF application by setting executor:Application.Current.Dispatcher.Invoke</param>
        public ReflectionMessageRouter(string handleMethodName = "HandleMessage", Action<Action>? executor = null)
        {
            this.handleMethodName = handleMethodName;
            this.executor = executor;
        }

        public void RouteMessage(object message, IMessenger messenger)
        {
            if (MessageHandler is null)
            {
                throw new Exception($"No {nameof(MessageHandler)} set");
            }

            var argTypes = new Type[] { message.GetType(), typeof(IMessenger) };

            var handleMethod = MessageHandler.GetType().GetMethod(handleMethodName, argTypes);
            if (handleMethod == null)
            {
                throw new NotImplementedException($"Message handling method ({handleMethodName}) not implemented for message type {message.GetType()}");
            }

            var args = new object[] { message, messenger };
            if (executor is not null)
            {
                executor.Invoke(() => handleMethod.Invoke(MessageHandler, args));
            }
            else
            {
                handleMethod.Invoke(MessageHandler, args);
            }
        }
    }
}
