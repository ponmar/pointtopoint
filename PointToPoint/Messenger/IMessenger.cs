using System;

namespace PointToPoint.Messenger
{
    public record MessengerDisconnected(Guid MessengerId, Exception? Exception);

    /// <summary>
    /// Handles sending and receiving of objects via TCP
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// A unique ID for this communication channel
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Starts the sending and receiving communication threads
        /// </summary>
        void Start();

        /// <summary>
        /// Closes this communication channel
        /// </summary>
        /// This method should not be called from a disconnected-callback-thread.
        /// The method blocks until the communication threads has been shut down.
        void Close();

        /// <summary>
        /// Queue a message for sending
        /// </summary>
        /// <param name="message">Any object that is included in the protocol namespace</param>
        void Send(object message);

        /// <summary>
        /// Fired when disconnected
        /// </summary>
        /// The event is fired from an internal communication thread.
        event EventHandler<MessengerDisconnected> Disconnected;
    }
}
