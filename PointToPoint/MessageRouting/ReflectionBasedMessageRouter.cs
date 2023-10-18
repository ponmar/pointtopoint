using System;

namespace PointToPoint.MessageRouting
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class MessageReceiverAttribute : Attribute
    {
    }

    /// <summary>
    /// Handles forwarding of messages to the specified handler class.
    /// </summary>
    /// The handler class needs to implement one method per handled message according to:
    ///
    /// [MessageReceiver]
    /// void HandleMessage(MyMessage message)
    /// {
    ///     // Message handling code
    /// }
    public class ReflectionBasedMessageRouter : IMessageRouter
    {
        public object MessageHandler { get; set; }

        public bool RouteMessage(object message)
        {
            var argTypes = new Type[] { message.GetType() };

            var handleMethod = MessageHandler.GetType().GetMethod("HandleMessage", argTypes);
            if (handleMethod == null)
            {
                return false;
            }

            var attributes = handleMethod.GetCustomAttributes(typeof(MessageReceiverAttribute), false);
            if (attributes.Length != 1)
            {
                return false;
            }

            var args = new Object[] { message };
            handleMethod.Invoke(MessageHandler, args);
            return true;
        }
    }
}
