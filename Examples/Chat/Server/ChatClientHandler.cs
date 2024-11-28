using PointToPoint.Server;
using PointToPoint.Protocol;
using Protocol;
using PointToPoint.Messenger;
using PointToPoint.Server.ClientHandler;
using PointToPoint.MessageRouting;

namespace Server;

public class ChatClientHandler : IClientHandler
{
    private const string ServerName = "Server";

    private readonly ClientRepo clientRepo = ClientRepo.Instance;

    private IMessageSender? messageSender;
    private QueuingReflectionMessageRouter? messageRouter;
    public string Name { get; private set; } = NameCreator.CreateName();

    public void Init(IMessageSender messageSender, IMessageRouter messageRouter)
    {
        this.messageSender = messageSender;
        this.messageRouter = (QueuingReflectionMessageRouter)messageRouter;

        clientRepo.AddClient(this);

        messageSender.SendMessage(new AssignName(Name), this);

        var text = $"'{Name}' joined the chat";
        messageSender.SendBroadcast(new Text(ServerName, text, DateTime.Now));
        Console.WriteLine(text);

        BroadcastUsers();
    }

    private void BroadcastUsers()
    {
        messageSender!.SendBroadcast(new Users(clientRepo.GetUsernames()));
    }

    public void Exit(Exception? e)
    {
        ClientRepo.Instance.RemoveClient(this);

        var text = $"'{Name}' left the chat";
        if (e is not null)
        {
            text += $" ({e.Message})";
        }
        Console.WriteLine(text);
        messageSender!.SendBroadcast(new Text(ServerName, text, DateTime.Now));

        BroadcastUsers();
    }

    public void Update()
    {
        if (messageRouter is not null)
        {
            while (messageRouter.HandleMessage())
            {
            }
        }
    }

    public void HandleMessage(PublishText message, IMessenger messenger)
    {
        Console.WriteLine($"'{Name}': {message.Message}");
        messageSender!.SendBroadcast(new Text(Name, message.Message, DateTime.Now));

        switch (message.Message.ToLower())
        {
            case "hi server":
            case "hello server":
                messageSender.SendBroadcast(new Text(ServerName, "And hi to you!", DateTime.Now));
                break;
        }
    }

    public void HandleMessage(ChangeName message, IMessenger messenger)
    {
        if (message.NewName != Name && !string.IsNullOrEmpty(message.NewName) && message.NewName.Length < 50)
        {
            var text = $"'{Name}' -> '{message.NewName}'";
            Console.WriteLine(text);
            messageSender!.SendBroadcast(new Text(ServerName, text, DateTime.Now));
            Name = message.NewName;
            BroadcastUsers();
        }
        else
        {
            messageSender!.SendMessage(new Text(ServerName, "Invalid name", DateTime.Now), this);
        }
    }

    public void HandleMessage(KeepAlive message, IMessenger messenger)
    {
    }
}
