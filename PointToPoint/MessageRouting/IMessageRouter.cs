using PointToPoint.Messenger;

namespace PointToPoint.MessageRouting
{
    /// <summary>
    /// The message router is responsible for delivering the message to the wanted recepient class.
    /// </summary>
    public interface IMessageRouter
    {
        /// <summary>
        /// Note that this method is called from the internal message receiving thread.
        /// </summary>
        public void RouteMessage(object message, IMessenger messenger);

        public void Update();
    }
}