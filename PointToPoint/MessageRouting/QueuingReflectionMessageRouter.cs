using PointToPoint.Messenger;
using System;
using System.Collections.Concurrent;

namespace PointToPoint.MessageRouting
{
    /// <summary>
    /// Handles forwarding of messages to the specified handler class via a thread safe queue.
    /// </summary>
    /// The message router must be polled for new messages to be handled on the application main thread.
    /// The handler class needs to implement one method per handled message according to:
    ///
    /// void HandleMessage(MyMessage message, IMessenger messenger)
    /// {
    ///     // Message handling code
    /// }
    /// 
    /// Note that the handle method is called from the internal message receiver thread when no executor is specified.
    public class QueuingReflectionMessageRouter : IMessageRouter
    {
        private const string handleMethodName = "HandleMessage";

        private readonly object messageHandler;

        private readonly ConcurrentQueue<Action> messageQueue = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageHandler">Instance that implements the messeage handling methods</param>
        public QueuingReflectionMessageRouter(object messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public void RouteMessage(object message, IMessenger messenger)
        {
            var argTypes = new Type[] { message.GetType(), typeof(IMessenger) };

            var handleMethod = messageHandler.GetType().GetMethod(handleMethodName, argTypes);
            if (handleMethod == null)
            {
                throw new NotImplementedException($"Message handling method ({handleMethodName}) not implemented for message type {message.GetType()}");
            }

            var args = new object[] { message, messenger };
            messageQueue.Enqueue(() => handleMethod.Invoke(messageHandler, args));
        }

        /// <summary>
        /// Thread safe method for handling messages queued from a messenger thread.
        /// </summary>
        /// <returns>true if a queued action was handled, otherwise false</returns>
        public bool HandleMessage()
        {
            if (messageQueue.TryDequeue(out var messageAction))
            {
                messageAction();
                return true;
            }
            return false;
        }
    }
}
