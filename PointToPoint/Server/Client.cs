using PointToPoint.Messenger;
using PointToPoint.Server.ClientHandler;
using System;

namespace PointToPoint.Server
{
    public class Client : IClient
    {
        public IClientHandler ClientHandler { get; }
        public IMessenger Messenger { get; }
        public IMessageBroadcaster MessageBroadcaster { get; }

        private readonly Action<Exception?> disconnectAction;
        private volatile bool initialized = false;

        public Client(IClientHandler clientHandler, IMessenger messenger, IMessageBroadcaster messageBroadcaster, Action<Exception?> disconnectAction)
        {
            ClientHandler = clientHandler;
            Messenger = messenger;
            MessageBroadcaster = messageBroadcaster;
            this.disconnectAction = disconnectAction;
        }

        public void Init()
        {
            ClientHandler.Init(this);
            initialized = true;
        }

        public void Update()
        {
            if (initialized)
            {
                Messenger.Update();
                ClientHandler.Update();
            }
        }

        public void Disconnect(Exception? e = null)
        {
            disconnectAction(e);
        }
    }
}
