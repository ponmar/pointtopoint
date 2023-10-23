using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PointToPoint.Server
{
    record Client(IMessenger Messenger, IAppClientMessageHandler MessageHandler);

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
        /// <param name="keepAliveSendInterval"></param>
        /// <param name="clientMessageHandlerType">Type of the class that will be created for handling application specific code per client</param>
        public ClientsHandler(IPayloadSerializer payloadSerializer, TimeSpan keepAliveSendInterval, Type clientMessageHandlerType)
        {
            this.payloadSerializer = payloadSerializer;
            this.keepAliveSendInterval = keepAliveSendInterval;
            this.clientMessageHandlerType = clientMessageHandlerType;
        }

        public void NewConnection(ISocket socket)
        {
            var clientMessageHandler = (IAppClientMessageHandler)Activator.CreateInstance(clientMessageHandlerType);
            var messageRouter = new ReflectionMessageRouter(clientMessageHandler);
            var messenger = new TcpMessenger(socket, payloadSerializer, messageRouter, keepAliveSendInterval);
            var client = new Client(messenger, clientMessageHandler);
            AddClient(client);
            clientMessageHandler.Init(this);
            messenger.Start();
        }

        public void SendMessage(object message, IAppClientMessageHandler sender)
        {
            Client? client = null;
            lock (clientsLock)
            {
                client = Clients.FirstOrDefault(x => x.MessageHandler == sender);
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

        private void Client_Disconnected(object sender, Exception? _)
        {
            RemoveClient((IMessenger)sender);
        }

        private void AddClient(Client client)
        {
            lock (clientsLock)
            {
                Clients.Add(client);
                client.Messenger.Disconnected += Client_Disconnected;
            }
        }

        private void RemoveClient(IMessenger clientMessenger)
        {
            bool clientRemoved;
            Client? client = null;
            lock (clientsLock)
            {
                client = Clients.FirstOrDefault(x => x.Messenger == clientMessenger);
                clientRemoved = client is not null && Clients.Remove(client);
            }

            if (clientRemoved)
            {
                client!.Messenger.Disconnected -= Client_Disconnected;
                if (client.MessageHandler is IDisposable disposableClient)
                {
                    disposableClient.Dispose();
                }
            }
        }
    }
}
