using PointToPoint.Messenger;
using PointToPoint.Server.ClientHandler;

namespace PointToPoint.Server
{
    public class Client : IClient
    {
        public IClientHandler ClientHandler { get; }
        public IMessenger Messenger { get; }
        public IMessageBroadcaster MessageBroadcaster { get; }

        private volatile bool initialized = false;

        public Client(IClientHandler clientHandler, IMessenger messenger, IMessageBroadcaster messageBroadcaster)
        {
            ClientHandler = clientHandler;
            Messenger = messenger;
            MessageBroadcaster = messageBroadcaster;
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
    }
}
