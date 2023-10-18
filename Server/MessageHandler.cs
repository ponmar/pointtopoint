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

        public void HandleMessage(Hello _, Guid senderId)
        {
            Console.WriteLine("Received message. Sending reply.");
            messageSender.SendMessage(new Hello(2), senderId);
        }
    }
}
