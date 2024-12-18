﻿using System;

namespace PointToPoint.Server.ClientHandler.Factories
{
    /// <summary>
    /// The default client handler factory that uses the Activator class to create client handler instances.
    /// </summary>
    public class ActivatorClientHandlerFactory : IClientHandlerFactory
    {
        /// <summary>
        /// Creates a client handler.
        /// </summary>
        /// <param name="clientHandlerType">The type that shall be instantiated per client connection. It must implement IClientHandler and have a parameterless constructor.</param>
        /// <returns>Client handler instance</returns>
        public IClientHandler Create<T>() where T : IClientHandler
        {
            return Activator.CreateInstance<T>();
        }
    }
}
