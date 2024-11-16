using System;
using PointToPoint.MessageRouting;

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
        /// <param name="messageSender">This instance can be used to send messages</param>
        /// <param name="messageRouter">The message router</param>
        void Init(IMessageSender messageSender, IMessageRouter messageRouter);

        /// <summary>
        /// Called when the client has disconnected
        /// </summary>
        /// This method executes on a messenger internal thread.
        /// <param name="e">The exception that caused the disconnect</param>
        void Exit(Exception? e);

        /// <summary>
        /// Can be used to handle queued messages when the QueuingReflectionMessageRouter is used.
        /// </summary>
        void Update();
    }
}
