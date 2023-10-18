using PointToPoint.MessageRouting;
using PointToPoint.Network;
using Protocol.Messages;

namespace Server
{
    public class MessageHandler
    {
        private readonly IClientHandler clientHandler;

        public MessageHandler(IClientHandler clientHandler)
        {
            this.clientHandler = clientHandler;
        }

        [MessageReceiver]
        public void HandleMessage(Hello message)
        {
            Console.WriteLine("Received message. Sending reply.");
            // TODO: only reply to the appropriate client
            clientHandler.Clients.ForEach(x => x.Send(new Hello(2)));
        }
    }
}
