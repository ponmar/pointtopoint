using System;

namespace PointToPoint.Messenger
{
    /// <summary>
    /// Handles sending and receiving of objects via TCP
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Starts the sending and receiving communication threads
        /// </summary>
        void Start();

        /// <summary>
        /// Indicate that the internal communication threads shall stop
        /// </summary>
        /// This method should not be called from a disconnected-callback-thread.
        void Stop();

        /// <summary>
        /// Check if the internal communication threads have stopped
        /// </summary>
        /// <returns></returns>
        bool IsStopped();

        /// <summary>
        /// Queue a message for sending
        /// </summary>
        /// <param name="message">Any object that is included in the protocol namespace</param>
        void Send(object message);

        /// <summary>
        /// Fired when disconnected
        /// </summary>
        /// The event is fired from an internal communication thread.
        event EventHandler<Exception?> Disconnected;

        public TimeSpan KeepAliveSendInterval { get; set; }
    }
}
