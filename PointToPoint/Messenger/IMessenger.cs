using System;

namespace PointToPoint.Messenger
{
    /// <summary>
    /// Handles sending and receiving of objects via TCP
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Called internally to start the sending and receiving communication loops
        /// </summary>
        void Start();

        /// <summary>
        /// Indicate that the internal communication loops shall stop
        /// </summary>
        /// This method should not be called from a disconnected-callback-thread.
        void Stop();

        /// <summary>
        /// Check if the internal communication loops have stopped
        /// </summary>
        /// <returns></returns>
        bool IsStopped();

        /// <summary>
        /// Queue a message for sending
        /// </summary>
        /// <param name="message">Any object that is included in the protocol namespace</param>
        void Send(object message);

        /// <summary>
        /// Fired when the connection is lost unexpectedly.
        /// </summary>
        /// The event is fired from an internal communication loop. A normal <see cref="Stop"/> call
        /// shuts down the messenger without raising this event.
        event EventHandler<Exception> Disconnected;

        public TimeSpan KeepAliveSendInterval { get; set; }

        /// <summary>
        /// Called internally to handle queued message handling for some message routers
        /// </summary>
        void Update();
    }
}
