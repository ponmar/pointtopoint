using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PointToPoint.Server
{
    public class ClientHandler : IClientHandler, IMessageSender
    {
        public List<IMessenger> Clients { get; } = new();
        private readonly object clientsLock = new();

        private readonly IPayloadSerializer payloadSerializer;
        private readonly IMessageRouter messageRouter;
        private readonly TimeSpan keepAliveSendInterval;

        // Note: this event is fired from the internal server socket accept thread
        public EventHandler<Guid>? ClientConnected;

        // Note: this event is fired from one of the internal socket communication threads
        public EventHandler<Guid>? ClientDisconnected;

        public ClientHandler(IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, TimeSpan keepAliveSendInterval)
        {
            this.payloadSerializer = payloadSerializer;
            this.messageRouter = messageRouter;
            this.keepAliveSendInterval = keepAliveSendInterval;
        }

        public void NewConnection(ISocket socket)
        {
            var client = new TcpMessenger(socket, payloadSerializer, messageRouter, keepAliveSendInterval);
            AddClient(client);
            client.Start();
            ClientConnected?.Invoke(this, client.Id);
        }

        public void SendMessage(object message, Guid receiverId)
        {
            IMessenger? client = null;
            lock (clientsLock)
            {
                client = Clients.FirstOrDefault(x => x.Id == receiverId);
            }
            client?.Send(message);
        }

        public void SendBroadcast(object message)
        {
            lock (clientsLock)
            {
                Clients.ForEach(x => x.Send(message));
            }
        }

        private void Client_Disconnected(object sender, MessengerDisconnected disconnected)
        {
            RemoveClient((IMessenger)sender);
        }

        private void AddClient(IMessenger client)
        {
            lock (clientsLock)
            {
                Clients.Add(client);
                client.Disconnected += Client_Disconnected;
            }
        }

        private void RemoveClient(IMessenger client)
        {
            bool clientRemoved;
            lock (clientsLock)
            {
                clientRemoved = Clients.Remove(client);
            }

            if (clientRemoved)
            {
                client.Disconnected -= Client_Disconnected;
                ClientDisconnected?.Invoke(this, client.Id);
            }
        }
    }
}
