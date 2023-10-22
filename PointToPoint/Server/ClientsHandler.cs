using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PointToPoint.Server
{
    record Client(IMessenger Messenger, object MessageHandler);

    /// <summary>
    /// Keeps track of all connected clients and creates one application specific message handling instance per client connection
    /// </summary>
    public class ClientsHandler : IConnectionHandler, IMessageSender
    {
        private List<Client> Clients { get; } = new();
        private readonly object clientsLock = new();

        private readonly IPayloadSerializer payloadSerializer;
        private readonly TimeSpan keepAliveSendInterval;

        private readonly Type clientMessageHandlerType;

        public ClientsHandler(IPayloadSerializer payloadSerializer, TimeSpan keepAliveSendInterval, Type clientMessageHandlerType)
        {
            this.payloadSerializer = payloadSerializer;
            this.keepAliveSendInterval = keepAliveSendInterval;
            this.clientMessageHandlerType = clientMessageHandlerType;
        }

        public void NewConnection(ISocket socket)
        {
            object clientMessageHandler = Activator.CreateInstance(clientMessageHandlerType, new object[] { this });
            var messageRouter = new ReflectionMessageRouter(clientMessageHandler);
            var messenger = new TcpMessenger(socket, payloadSerializer, messageRouter, keepAliveSendInterval);
            var client = new Client(messenger, clientMessageHandler);
            AddClient(client);
            messenger.Start();
        }

        public void SendMessage(object message, Guid receiverId)
        {
            Client? client = null;
            lock (clientsLock)
            {
                client = Clients.FirstOrDefault(x => x.Messenger.Id == receiverId);
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

        private void Client_Disconnected(object sender, MessengerDisconnected disconnected)
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
