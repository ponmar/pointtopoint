using PointToPoint.Messenger;
using PointToPoint.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly Dictionary<Type, Action<object, IMessenger>> routeActions = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageHandler">Instance that implements the message handling methods</param>
        /// <param name="executor">Can be used to route message to the UI thread (in WPF: Application.Current.Dispatcher.Invoke, in Avalonia: Avalonia.Threading.Dispatcher.UIThread.Invoke)</param>
        public ReflectionMessageRouter(object messageHandler, Action<Action>? executor = null)
        {
            var handleMessageMethods = messageHandler.GetType().GetMethods().
                Where(m => m.Name == handleMethodName && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(IMessenger));
            foreach (var handleMessageMethod in handleMessageMethods)
            {
                var messageType = handleMessageMethod.GetParameters()[0].ParameterType;

                Action<object, IMessenger> routeAction = executor is null ?
                    (o, m) => handleMessageMethod.Invoke(messageHandler, new object[] { o, m }) :
                    (o, m) => executor.Invoke(() => handleMessageMethod.Invoke(messageHandler, new object[] { o, m }));

                routeActions[messageType] = routeAction;
            }

            if (!routeActions.ContainsKey(typeof(KeepAlive)))
            {
                throw new NotImplementedException($"Message handling method ({handleMethodName}) not implemented for message type {typeof(KeepAlive)}");
            }
        }

        public void RouteMessage(object message, IMessenger messenger)
        {
            if (!routeActions.TryGetValue(message.GetType(), out var routeAction))
            {
                throw new NotImplementedException($"Message handling method ({handleMethodName}) not implemented for message type {message.GetType()}");
            }
            DoRouteMessage(() => routeAction(message, messenger));
        }

        protected virtual void DoRouteMessage(Action routeAction) => routeAction();

        public virtual void Update() { }
    }
}
