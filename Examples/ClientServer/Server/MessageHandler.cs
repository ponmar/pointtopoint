using PointToPoint.Server;
using PointToPoint.Protocol;
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

        public void HandleMessage(Hello message, Guid senderId)
        {
            Console.WriteLine($"Received message {message.GetType()}. Sending reply.");
            messageSender.SendMessage(new Hello(2), senderId);
        }

        public void HandleMessage(KeepAlive message, Guid senderId)
        {
            Console.WriteLine("Received KeepAlive");
        }
    }
}
