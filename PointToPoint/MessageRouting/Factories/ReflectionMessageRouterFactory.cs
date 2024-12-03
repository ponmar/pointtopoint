using System;

namespace PointToPoint.MessageRouting.Factories
{
    public class ReflectionMessageRouterFactory : IMessageRouterFactory
    {
        private readonly Action<Action>? executor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="executor">Can be used to route message on the UI thread in a WPF application by setting executor:Application.Current.Dispatcher.Invoke</param></param>
        public ReflectionMessageRouterFactory(Action<Action>? executor = null)
        {
            this.executor = executor;
        }

        public IMessageRouter Create(object clientHandler)
        {
            return new ReflectionMessageRouter(clientHandler, executor);
        }
    }
}
