using PointToPoint.Server;
using PointToPoint.Protocol;
using Protocol;
using PointToPoint.Messenger;

namespace Server;

public class ChatClientHandler : IAppClientMessageHandler, IDisposable
{
    private const string ServerName = "Server";

    private IMessageSender? messageSender;
    private string name = NameCreator.CreateName();

    public void Init(IMessageSender messageSender)
    {
        this.messageSender = messageSender;
        messageSender.SendMessage(new AssignName(name), this);
        var text = $"'{name}' joined the chat";
        messageSender.SendBroadcast(new Text(ServerName, text, DateTime.Now));
        Console.WriteLine(text);
    }

    void IDisposable.Dispose()
    {
        var text = $"'{name}' left the chat";
        Console.WriteLine(text);
        messageSender!.SendBroadcast(new Text(ServerName, text, DateTime.Now));
    }

    public void HandleMessage(PublishText message, IMessenger messenger)
    {
        Console.WriteLine($"'{name}': {message.Message}");
        messageSender!.SendBroadcast(new Text(name, message.Message, DateTime.Now));

        switch (message.Message.ToLower())
        {
            case "hi server":
            case "hello server":
                messageSender.SendBroadcast(new Text(ServerName, $"And hi to you!", DateTime.Now));
                break;
        }
    }

    public void HandleMessage(ChangeName message, IMessenger messenger)
    {
        if (message.NewName != name && !string.IsNullOrEmpty(message.NewName) && message.NewName.Length < 50)
        {
            var text = $"{name} -> {message.NewName}";
            Console.WriteLine(text);
            messageSender!.SendBroadcast(new Text(ServerName, text, DateTime.Now));
            name = message.NewName;
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
