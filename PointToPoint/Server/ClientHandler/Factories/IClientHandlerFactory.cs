using System;

namespace PointToPoint.Server.ClientHandler.Factories
{
    /// <summary>
    /// The factory interface for creation of server side client handler
    /// </summary>
    /// A custom factory can be implemented to, for example, create the IClientHandler via a service locator.
    public interface IClientHandlerFactory
    {
        IClientHandler Create<T>() where T : IClientHandler;
    }
}
