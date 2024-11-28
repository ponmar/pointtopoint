using PointToPoint.MessageRouting.Factories;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using PointToPoint.Server.ClientHandler;
using PointToPoint.Server.ClientHandler.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PointToPoint.Server
{
    record Client(IMessenger Messenger, IClientHandler ClientHandler);

    /// <summary>
    /// Keeps track of all connected clients and creates one application specific message handling instance per client connection.
    /// </summary>
    /// This class sets up an ReflectionMessageRouter instance to forward messages to the application specific code.
    public class ClientsHandler<TClientHandler> : IConnectionHandler, IMessageSender where TClientHandler : IClientHandler
    {
        private List<Client> Clients { get; } = new();
        private readonly object clientsLock = new();

        private readonly IPayloadSerializer payloadSerializer;
        private readonly IClientHandlerFactory clientHandlerFactory;
        private readonly IMessageRouterFactory messageRouterFactory;

        public TimeSpan KeepAliveSendInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="payloadSerializer"></param>
        /// <param name="clientMessageHandlerType">Type of the class that will be created for handling application specific code per client</param>
        /// <param name="clientHandlerFactory">The factory for creating the client handler instance per connected client</param>
        /// <param name="keepAliveSendInterval"></param>
        public ClientsHandler(IPayloadSerializer payloadSerializer, IClientHandlerFactory clientHandlerFactory, IMessageRouterFactory messageRouterFactory)
        {
            this.payloadSerializer = payloadSerializer;
            this.clientHandlerFactory = clientHandlerFactory;
            this.messageRouterFactory = messageRouterFactory;
        }

        public void Stop()
        {
            lock (clientsLock)
            {
                Clients.ForEach(x => x.Messenger.Stop());
            }
        }

        public bool IsStopped()
        {
            lock (clientsLock)
            {
                return Clients.All(x => x.Messenger.IsStopped());
            }
        }

        public void NewConnection(ISocket socket)
        {
            var clientHandler = clientHandlerFactory.Create<TClientHandler>();
            var messageRouter = messageRouterFactory.Create(clientHandler);
            var messenger = new TcpMessenger(socket, payloadSerializer, messageRouter)
            {
                KeepAliveSendInterval = KeepAliveSendInterval
            };
            var client = new Client(messenger, clientHandler);
            AddClient(client);
            clientHandler.Init(this, messageRouter);
            messenger.Start();
        }

        public void SendMessage(object message, IClientHandler sender)
        {
            Client? client = null;
            lock (clientsLock)
            {
                client = Clients.FirstOrDefault(x => x.ClientHandler == sender);
            }
            client?.Messenger.Send(message);
        }

        public void SendBroadcast(object message)
        {
            lock (clientsLock)
            {
                Clients.ForEach(x => x.Messenger.Send(message));
            }
        }

        private void Client_Disconnected(object sender, Exception? e)
        {
            RemoveClient((IMessenger)sender, e);
        }

        private void AddClient(Client client)
        {
            lock (clientsLock)
            {
                Clients.Add(client);
                client.Messenger.Disconnected += Client_Disconnected;
            }
        }

        private void RemoveClient(IMessenger clientMessenger, Exception? e)
        {
            Client? client = null;
            lock (clientsLock)
            {
                client = Clients.FirstOrDefault(x => x.Messenger == clientMessenger);
                if (client is not null)
                {
                    Clients.Remove(client);
                }
            }

            if (client is not null)
            {
                client.Messenger.Disconnected -= Client_Disconnected;
                client.Messenger.Stop();
                client.ClientHandler.Exit(e);
            }
        }

        public void UpdateAll()
        {
            lock (clientsLock)
            {
                Clients.ForEach(x => x.ClientHandler.Update());
            }
        }
    }
}
