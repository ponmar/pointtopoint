using PointToPoint.Messenger;
using System;
using PointToPoint.Server.ClientHandler;

namespace PointToPoint.MessageRouting.Factories
{
    public class CustomMessageRouterFactory : IMessageRouterFactory
    {
        private readonly Action<object, IMessenger> routeAction;

        public CustomMessageRouterFactory(Action<object, IMessenger> routeAction)
        {
            this.routeAction = routeAction;
        }

        public IMessageRouter Create(IClientHandler clientHandler)
        {
            return new CustomMessageRouter(routeAction);
        }
    }
}
