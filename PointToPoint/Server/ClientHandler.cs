using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Messenger.ErrorHandler;
using PointToPoint.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace PointToPoint.Server
{
    public class ClientHandler : IClientHandler, IMessageSender, IMessengerErrorHandler
    {
        public List<IMessenger> Clients { get; } = new();
        private readonly object clientsLock = new();

        private readonly IPayloadSerializer payloadSerializer;
        private readonly IMessageRouter messageRouter;
        private readonly IMessengerErrorHandler errorLogger;

        // Note: this event is fired from the server socket accept thread
        public EventHandler<Guid> ClientConnected;

        // Note: this event is fired from one of the socket communication threads
        public EventHandler<Guid> ClientDisconnected;

        public ClientHandler(IPayloadSerializer payloadSerializer, IMessageRouter messageRouter, IMessengerErrorHandler errorLogger)
        {
            this.payloadSerializer = payloadSerializer;
            this.messageRouter = messageRouter;
            this.errorLogger = errorLogger;
        }

        public void NewConnection(Socket socket)
        {
            var client = new TcpMessenger(socket, payloadSerializer, messageRouter, this);
            
            lock (clientsLock)
            {
                Clients.Add(client);
            }
            
            client.Start();
            ClientConnected?.Invoke(this, client.Id);
        }

        public void SendMessage(object message, Guid receiverId)
        {
            IMessenger client = null;
            lock (clientsLock)
            {
                client = Clients.FirstOrDefault(x => x.Id == receiverId);
            }
            if (client is not null)
            {
                client.Send(message);
            }
        }

        public void SendBroadcast(object message)
        {
            lock (clientsLock)
            {
                Clients.ForEach(x => x.Send(message));
            }
        }

        public void PayloadException(Exception e, Guid messengerId)
        {
            if (RemoveClient(messengerId))
            {
                errorLogger.PayloadException(e, messengerId);
            }
        }

        public void MessageRoutingException(Exception e, Guid messengerId)
        {
            if (RemoveClient(messengerId))
            {
                errorLogger.MessageRoutingException(e, messengerId);
            }
        }

        public void Disconnected(Guid messengerId)
        {
            if (RemoveClient(messengerId))
            {
                errorLogger.Disconnected(messengerId);
            }
        }

        private bool RemoveClient(Guid clientId)
        {
            IMessenger client;
            bool clientRemoved;
            lock (clientsLock)
            {
                client = Clients.FirstOrDefault(x => x.Id == clientId);
                clientRemoved = client is not null && Clients.Remove(client);
            }

            if (clientRemoved)
            {
                ClientDisconnected?.Invoke(this, client.Id);
            }            
            return clientRemoved;
        }
    }
}
