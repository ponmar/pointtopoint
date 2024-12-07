using System;
using System.Collections.Concurrent;

namespace PointToPoint.MessageRouting
{
    /// <summary>
    /// Same callback handling as for ReflectionMessageRouter, but message handling is queued and handled via ClientsHandler.UpdateClients
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
                
        public override void Update()
        {
            while (routeActions.TryDequeue(out var messageAction))
            {
                messageAction();
            }
        }
    }
}
