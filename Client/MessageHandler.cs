using PointToPoint.Network;
using Protocol.Messages;

namespace Client
{
    public class MessageHandler : IMessengerErrorHandler
    {
        public void HandleMessage(Hello message, Guid senderId)
        {
            Console.WriteLine("Received message");
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
