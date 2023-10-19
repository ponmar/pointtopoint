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
    /// Note that the handle method is called on the internal message receiver thread.
    public class ReflectionMessageRouter : IMessageRouter
    {
        private readonly string handleMethodName;

        public object MessageHandler { get; set; }

        public ReflectionMessageRouter(string handleMethodName = "HandleMessage")
        {
            this.handleMethodName = handleMethodName;
        }

        public void RouteMessage(object message, Guid senderId)
        {
            var argTypes = new Type[] { message.GetType(), typeof(Guid) };

            var handleMethod = MessageHandler.GetType().GetMethod(handleMethodName, argTypes);
            if (handleMethod == null)
            {
                throw new Exception($"Message handling method ({handleMethodName}) not implemented for message type {message.GetType()}");
            }

            var args = new object[] { message, senderId };
            handleMethod.Invoke(MessageHandler, args);
        }
    }
}
