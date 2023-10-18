using PointToPoint.MessageRouting;
using PointToPoint.Network;
using PointToPoint.Payload;
using System.Net.Sockets;

namespace Server
{
    public interface IMessageSender
    {
        void SendMessage(object message, Guid receiverId);
        void SendMessage(object message, TcpMessenger receiver);
        void SendMessageToAll(object message);
    }

    public class ClientHandler : IClientHandler, IMessageSender
    {
        public List<TcpMessenger> Clients { get; } = new();

        private readonly IPayloadSerializer payloadSerializer;
        private readonly string messagesNamespace;
        private readonly IMessageRouter messageRouter;

        public ClientHandler(IPayloadSerializer payloadSerializer, string messagesNamespace, IMessageRouter messageRouter)
        {
            this.payloadSerializer = payloadSerializer;
            this.messagesNamespace = messagesNamespace;
            this.messageRouter = messageRouter;
        }

        public void NewConnection(Socket socket)
        {
            Console.WriteLine("Client connected");
            var client = new TcpMessenger(socket, payloadSerializer, messagesNamespace, messageRouter);
            Clients.Add(client);
            client.StartCommunication();
        }

        public void SendMessage(object message, Guid receiverId)
        {
            var client = Clients.FirstOrDefault(x => x.Id == receiverId);
            if (client is not null)
            {
                client.Send(message);
            }
        }

        public void SendMessage(object message, TcpMessenger receiver)
        {
            receiver.Send(message);
        }

        public void SendMessageToAll(object message)
        {
            Clients.ForEach(x => x.Send(message));
        }
    }
}
