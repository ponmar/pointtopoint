using PointToPoint.Messenger;
using System;

namespace PointToPoint.MessageRouting.Factories
{
    public class CustomMessageRouterFactory : IMessageRouterFactory
    {
        private readonly Action<object, IMessenger> routeAction;

        public CustomMessageRouterFactory(Action<object, IMessenger> routeAction)
        {
            this.routeAction = routeAction;
        }

        public IMessageRouter Create(object clientHandler)
        {
            return new CustomMessageRouter(routeAction);
        }
    }
}
