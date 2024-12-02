using PointToPoint.Server;
using Protocol;
using PointToPoint.Messenger;
using PointToPoint.Server.ClientHandler;
using PointToPoint.MessageRouting;
using PointToPoint.Protocol;

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
        PublishText(ServerName, text);
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
        PublishText(ServerName, text);
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
        PublishText(Name, message.Message);

        switch (message.Message.ToLower())
        {
            case "hi server":
            case "hello server":
                PublishText(ServerName, "And hi to you!");
                break;
        }
    }

    public void HandleMessage(ChangeName message, IMessenger messenger)
    {
        if (message.NewName != Name && !string.IsNullOrEmpty(message.NewName) && message.NewName.Length < 50)
        {
            var text = $"'{Name}' -> '{message.NewName}'";
            Console.WriteLine(text);
            PublishText(ServerName, text);
            Name = message.NewName;
            BroadcastUsers();
        }
        else
        {
            PublishText(ServerName, "Invalid name");
        }
    }

    public void HandleMessage(KeepAlive message, IMessenger messenger)
    {
    }

    private void PublishText(string sender, string text)
    {
        messageSender!.SendBroadcast(new Text(sender, text, DateTime.Now));
    }
}
