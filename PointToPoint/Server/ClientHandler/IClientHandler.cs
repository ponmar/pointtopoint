using System;
using PointToPoint.MessageRouting;

namespace PointToPoint.Server.ClientHandler
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
        /// <param name="messageSender">This instance can be used to send messages</param>
        /// <param name="messageRouter">The message router</param>
        void Init(IMessageSender messageSender, IMessageRouter messageRouter);

        /// <summary>
        /// Called when the client has disconnected
        /// </summary>
        /// <param name="e">The exception that caused the disconnect</param>
        void Exit(Exception? e);

        /// <summary>
        /// Can be used to handle queued messages when the QueuingReflectionMessageRouter is used.
        /// </summary>
        void Update();
    }
}
