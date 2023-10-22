using PointToPoint.Server;
using PointToPoint.Protocol;
using Protocol;
using PointToPoint.Messenger;

namespace Server;

public class ChatClientHandler : IAppClientMessageHandler, IDisposable
{
    private const string ServerName = "Server";

    private IMessageSender? messageSender;
    private readonly string name = NameCreator.CreateName();

    public void Init(IMessageSender messageSender)
    {
        this.messageSender = messageSender;
        Console.WriteLine($"{name} connected");
        messageSender.SendBroadcast(new Text(ServerName, $"'{name}' joined", DateTime.Now));
    }


    void IDisposable.Dispose()
    {
        Console.WriteLine($"{name} disconnected");
    }

    public void HandleMessage(PublishText message, IMessenger messenger)
    {
        Console.WriteLine($"Forwarding message '{message.Message}' to all.");
        messageSender!.SendBroadcast(new Text(name, message.Message, DateTime.Now));

        switch (message.Message.ToLower())
        {
            case "hi":
            case "hello":
                Console.WriteLine($"Sending greeting.");
                messageSender.SendMessage(new Text(name, $"And {message.Message} to you! /Server", DateTime.Now), messenger.Id);
                break;
        }
    }

    public void HandleMessage(KeepAlive message, IMessenger messenger)
    {
    }
}
