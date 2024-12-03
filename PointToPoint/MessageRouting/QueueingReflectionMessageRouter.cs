using System;
using System.Collections.Concurrent;

namespace PointToPoint.MessageRouting
{
    /// <summary>
    /// Same callback handling as for ReflectionMessageRouter, but message handling is queued and polled from the application
    /// </summary>
    public class QueueingReflectionMessageRouter : ReflectionMessageRouter
    {
        private readonly ConcurrentQueue<Action> routeActions = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageHandler">Instance that implements the messeage handling methods</param>
        public QueueingReflectionMessageRouter(object messageHandler) : base(messageHandler)
        {
        }

        protected override void DoRouteMessage(Action routeAction) => routeActions.Enqueue(routeAction);

        /// <summary>
        /// Thread safe method for handling messages queued from a messenger thread.
        /// </summary>
        /// <returns>true if a queued action was handled, otherwise false</returns>
        public bool HandleMessage()
        {
            if (routeActions.TryDequeue(out var messageAction))
            {
                messageAction();
                return true;
            }
            return false;
        }
    }
}
