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

    public class ClientHandler : IClientHandler, IMessageSender, IMessengerErrorHandler
    {
        public List<IMessenger> Clients { get; } = new();

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
            var client = new TcpMessenger(socket, payloadSerializer, messagesNamespace, messageRouter, this);
            Clients.Add(client);
            client.Start();
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

        public void PayloadException(Exception e, Guid messengerId)
        {
            Console.WriteLine($"Payload exception: {e.Message}");
            CloseAndRemoveClient(messengerId);
        }

        public void NonProtocolMessageReceived(object message, Guid messengerId)
        {
            Console.WriteLine($"Non protocol message received: {message.GetType()}");
            CloseAndRemoveClient(messengerId);
        }

        public void MessageRoutingException(Exception e, Guid messengerId)
        {
            Console.WriteLine($"Message routing exception: {e.Message}");
            CloseAndRemoveClient(messengerId);
        }

        public void Disconnected(Guid messengerId)
        {
            Console.WriteLine("Client disconnected");
            CloseAndRemoveClient(messengerId);
        }

        private void CloseAndRemoveClient(Guid clientId)
        {
            var client = Clients.FirstOrDefault(x => x.Id == clientId);
            if (client is not null)
            {
                client.Close();
                Clients.Remove(client);
                Console.WriteLine("Client removed");
            }
        }
    }
}
