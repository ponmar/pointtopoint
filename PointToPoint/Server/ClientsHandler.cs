using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
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
    public class ClientsHandler : IConnectionHandler, IMessageSender
    {
        private List<Client> Clients { get; } = new();
        private readonly object clientsLock = new();

        private readonly IPayloadSerializer payloadSerializer;
        private readonly TimeSpan keepAliveSendInterval;

        private readonly Type clientMessageHandlerType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="payloadSerializer"></param>
        /// <param name="clientMessageHandlerType">Type of the class that will be created for handling application specific code per client</param>
        /// <param name="keepAliveSendInterval"></param>
        public ClientsHandler(IPayloadSerializer payloadSerializer, Type clientMessageHandlerType, TimeSpan keepAliveSendInterval)
        {
            this.payloadSerializer = payloadSerializer;
            this.keepAliveSendInterval = keepAliveSendInterval;
            this.clientMessageHandlerType = clientMessageHandlerType;

            if (!clientMessageHandlerType.GetInterfaces().Contains(typeof(IClientHandler)))
            {
                throw new ArgumentException($"{clientMessageHandlerType} does not implement {nameof(IClientHandler)}");
            }
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
            var clientMessageHandler = (IClientHandler)Activator.CreateInstance(clientMessageHandlerType);
            var messageRouter = new ReflectionMessageRouter(clientMessageHandler);
            var messenger = new TcpMessenger(socket, payloadSerializer, messageRouter, keepAliveSendInterval);
            var client = new Client(messenger, clientMessageHandler);
            AddClient(client);
            clientMessageHandler.Init(this);
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
    }
}
