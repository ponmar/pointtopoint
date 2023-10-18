using PointToPoint.MessageRouting;
using Protocol.Messages;

namespace Client
{
    public class MessageHandler
    {
        [MessageReceiver]
        public void HandleMessage(Hello message)
        {
            Console.WriteLine("Received message");
        }
    }
}
