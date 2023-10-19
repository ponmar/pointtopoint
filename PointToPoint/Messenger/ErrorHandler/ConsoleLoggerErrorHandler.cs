using System;

namespace PointToPoint.Messenger.ErrorHandler
{
    public class ConsoleLoggerErrorHandler : IMessengerErrorHandler
    {
        public void Disconnected(Guid messengerId)
        {
            Console.WriteLine("Client disconnected");
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
    }
}
