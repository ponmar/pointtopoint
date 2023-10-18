using PointToPoint.MessageRouting;
using PointToPoint.Network;
using PointToPoint.Payload;
using System.Net.Sockets;

namespace Server
{
    public class ClientHandler : IClientHandler
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

    }
}
