using Protocol.Messages;

namespace Client
{
    public class MessageHandler
    {
        public void HandleMessage(Hello message, Guid senderId)
        {
            Console.WriteLine("Received message");
        }
    }
}
