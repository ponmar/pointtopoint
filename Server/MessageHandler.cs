using PointToPoint.MessageRouting;
using Protocol.Messages;

namespace Server
{
    public class MessageHandler
    {
        private readonly IMessageSender messageSender;

        public MessageHandler(IMessageSender messageSender)
        {
            this.messageSender = messageSender;
        }

        [MessageReceiver]
        public void HandleMessage(Hello _)
        {
            Console.WriteLine("Received message. Sending reply.");
            // TODO: only reply to the appropriate client
            messageSender.SendMessageToAll(new Hello(2));
        }
    }
}
