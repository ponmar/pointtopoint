using PointToPoint.Messenger.ErrorHandler;
using PointToPoint.Protocol;
using Protocol;

namespace Client
{
    public class MessageHandler : IMessengerErrorReporter
    {
        public void HandleMessage(Hello message, Guid senderId)
        {
            Console.WriteLine($"Received message {message.GetType()}");
        }

        public void HandleMessage(KeepAlive message, Guid senderId)
        {
            Console.WriteLine($"Received KeepAlive");
        }

        public void Disconnected(Guid messengerId, Exception e)
        {
            Console.WriteLine($"{messengerId} disconnected from server: {e?.Message}");
        }
    }
}
