using System;

namespace PointToPoint.Server.ClientHandler
{
    /// <summary>
    /// The interface for server side specific data per client connection
    /// </summary>
    public interface IClientHandler
    {
        /// <summary>
        /// Called when messenger is ready
        /// </summary>
        /// This method executes on a messenger internal thread.
        /// <param name="client">TODO</param>
        void Init(IClient client);

        /// <summary>
        /// Called when the client has disconnected
        /// </summary>
        /// This method executes on a messenger internal thread.
        /// <param name="e">The exception that caused the disconnect</param>
        void Exit(Exception? e);

        /// <summary>
        /// This method is called continously
        /// </summary>
        void Update();
    }
}
