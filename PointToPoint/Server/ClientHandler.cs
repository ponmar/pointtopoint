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

        private readonly IPayloadSerializer payloadSerializer;
        private readonly string messagesNamespace;
        private readonly IMessageRouter messageRouter;
        private readonly IMessengerErrorHandler messengerErrorHandler;

        public ClientHandler(IPayloadSerializer payloadSerializer, string messagesNamespace, IMessageRouter messageRouter, IMessengerErrorHandler messengerErrorHandler)
        {
            this.payloadSerializer = payloadSerializer;
            this.messagesNamespace = messagesNamespace;
            this.messageRouter = messageRouter;
            this.messengerErrorHandler = messengerErrorHandler;
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

        public void SendBroadcast(object message)
        {
            Clients.ForEach(x => x.Send(message));
        }

        public void PayloadException(Exception e, Guid messengerId)
        {
            if (RemoveClient(messengerId))
            {
                messengerErrorHandler.PayloadException(e, messengerId);
            }
        }

        public void NonProtocolMessageReceived(object message, Guid messengerId)
        {
            if (RemoveClient(messengerId))
            {
                messengerErrorHandler.NonProtocolMessageReceived(message, messengerId);
            }
        }

        public void MessageRoutingException(Exception e, Guid messengerId)
        {
            if (RemoveClient(messengerId))
            {
                messengerErrorHandler.MessageRoutingException(e, messengerId);
            }
        }

        public void Disconnected(Guid messengerId)
        {
            if (RemoveClient(messengerId))
            {
                messengerErrorHandler.Disconnected(messengerId);
            }
        }

        private bool RemoveClient(Guid clientId)
        {
            var client = Clients.FirstOrDefault(x => x.Id == clientId);
            if (client is not null)
            {
                return Clients.Remove(client);
            }
            return false;
        }
    }
}
