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
    /// <summary>
    /// Keeps track of all connected clients and creates one application specific message handling instance per client connection.
    /// </summary>
    public class ClientsHandler<TClientHandler> : IConnectionHandler, IMessageBroadcaster where TClientHandler : IClientHandler
    {
        private List<Client> Clients { get; } = new();
        private readonly object clientsLock = new();

        private readonly IPayloadSerializer payloadSerializer;
        private readonly IClientHandlerFactory clientHandlerFactory;
        private readonly IMessageRouterFactory messageRouterFactory;

        public TimeSpan KeepAliveSendInterval { get; set; } = Messenger.Messenger.DefaultKeepAliveSendInterval;

        public SocketOptions SocketOptions { get; set; } = TcpMessenger.DefaultSocketOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="payloadSerializer"></param>
        /// <param name="clientHandlerFactory">The factory that creates the application specific instance handler per client</param>
        /// <param name="clientHandlerFactory">The factory for creating the client handler instance per connected client</param>
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
            var messenger = new TcpMessenger(socket, payloadSerializer, messageRouter, SocketOptions)
            {
                KeepAliveSendInterval = KeepAliveSendInterval
            };

            // Note that the order matters. The client shall be available in the list to make it possible
            // for the application specific client handler to broadcast to all (with itself included) in
            // its Init method.
            var client = new Client(clientHandler, messenger, this);
            AddClient(client);
            client.Init();
            messenger.Start();
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

        public void UpdateClients()
        {
            lock (clientsLock)
            {
                Clients.ForEach(x => x.Update());
            }
        }
    }
}
