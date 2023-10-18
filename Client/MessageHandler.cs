using Protocol.Messages;

namespace Client
{
    public class MessageHandler
    {
        public void HandleMessage(Hello message)
        {
            Console.WriteLine("Received message");
        }
    }
}
