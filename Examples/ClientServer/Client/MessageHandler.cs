using PointToPoint.Messenger.ErrorHandler;
using PointToPoint.Protocol;
using Protocol;

namespace Client
{
    public class MessageHandler : IMessengerErrorHandler
    {
        public void HandleMessage(Hello message, Guid senderId)
        {
            Console.WriteLine($"Received message {message.GetType()}");
        }

        public void HandleMessage(KeepAlive message, Guid senderId)
        {
            Console.WriteLine($"Received KeepAlive");
        }

        public void MessageRoutingException(Exception e, Guid messengerId)
        {
            Console.WriteLine($"Message routing exception: {e.Message}");
        }

        public void NonProtocolMessageReceived(object message, Guid messengerId)
        {
            Console.WriteLine($"Non protocol message received: {message.GetType()}");
        }

        public void PayloadException(Exception e, Guid messengerId)
        {
            Console.WriteLine($"Payload exception: {e.Message}");
        }

        public void Disconnected(Guid messengerId)
        {
            Console.WriteLine("Disconnected from server");
        }
    }
}
