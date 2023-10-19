using PointToPoint.Server;
using PointToPoint.Protocol;
using Protocol;

namespace Server;

public class MessageHandler
{
    private readonly IMessageSender messageSender;

    public MessageHandler(IMessageSender messageSender)
    {
        this.messageSender = messageSender;
    }

    public void HandleMessage(PublishText message, Guid senderId)
    {
        Console.WriteLine($"Forwarding message '{message.Message}' to all.");
        messageSender.SendBroadcast(new Text(message.Message, DateTime.Now));
    }

    public void HandleMessage(KeepAlive message, Guid senderId)
    {
    }
}
