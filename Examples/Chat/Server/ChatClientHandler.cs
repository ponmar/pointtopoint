using PointToPoint.Server;
using Protocol;
using PointToPoint.Messenger;
using PointToPoint.Server.ClientHandler;
using PointToPoint.Protocol;

namespace Server;

public class ChatClientHandler : IClientHandler
{
    private const string ServerName = "Server";

    private readonly ClientRepo clientRepo = ClientRepo.Instance;

    private IClient? client;

    public string Name { get; private set; } = NameCreator.CreateName();

    public void Init(IClient client)
    {
        this.client = client;

        clientRepo.AddClient(this);

        client.Messenger.Send(new AssignName(Name));

        var text = $"'{Name}' joined the chat";
        Console.WriteLine(text);

        PublishText(ServerName, text);
        PublishHelp();

        BroadcastUsers();
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
    }

    public void HandleMessage(PublishText message, IMessenger messenger)
    {
        if (message.Message.StartsWith("/"))
        {
            var commandParts = message.Message.Split(" ");
            var command = commandParts[0].ToLower();
            var commandArguments = commandParts[1..];
            HandleCommand(command, commandArguments);
            return;
        }
        else
        {
            Console.WriteLine($"'{Name}': {message.Message}");
            PublishText(Name, message.Message);
        }
    }

    private void HandleCommand(string command, IEnumerable<string> arguments)
    {
        switch (command)
        {
            case "/?":
            case "/help":
                PublishHelp();
                break;

            case "/hi":
            case "/hello":
                PublishText(ServerName, "And hi to you!");
                break;
            
            case "/name":
                var newName = string.Join(' ', arguments);
                ChangeName(newName);
                break;

            case "/quit":
                client?.Messenger.Stop();
                break;

            default:
                PublishText(ServerName, $"Invalid command: {command}");
                break;
        }
    }

    private void ChangeName(string newName)
    {
        if (newName != Name && !string.IsNullOrEmpty(newName) && newName.Length < 50)
        {
            var text = $"'{Name}' -> '{newName}'";
            Console.WriteLine(text);
            PublishText(ServerName, text);
            Name = newName;
            BroadcastUsers();
        }
        else
        {
            PublishText(ServerName, $"Invalid name: {newName}");
        }
    }

    public void HandleMessage(KeepAlive message, IMessenger messenger)
    {
    }

    private void PublishText(string sender, string text)
    {
        client?.Messenger!.Send(new Text(sender, text, DateTime.Now));
    }

    private void PublishHelp()
    {
        var commands = new List<string>()
        {
            "/?",
            "/help",
            "/hi",
            "/hello",
            "/name <name>",
            "/quit",
        };
        client?.Messenger!.Send(new Text(ServerName, string.Join('\n', commands), DateTime.Now));
    }

    private void BroadcastUsers()
    {
        client!.MessageBroadcaster.SendBroadcast(new Users(clientRepo.GetUsernames()));
    }
}
