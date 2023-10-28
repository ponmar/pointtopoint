using System;

namespace PointToPoint.Server
{
    /// <summary>
    /// The interface for server side specific data per client connection
    /// </summary>
    /// The Dispose method can be used to perform cleanup when a client is disconnected
    public interface IClientHandler
    {
        /// <summary>
        /// Called when messenger is ready
        /// </summary>
        /// <param name="messageSender"></param>
        void Init(IMessageSender messageSender);

        /// <summary>
        /// Called when the client has disconnected
        /// </summary>
        /// <param name="e">The exception that caused the disconnect</param>
        void Exit(Exception? e);
    }
}
