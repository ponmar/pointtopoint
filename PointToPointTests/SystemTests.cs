using PointToPoint.MessageRouting;
using PointToPoint.MessageRouting.Factories;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using PointToPointProtocol;
using PointToPoint.Server;
using PointToPoint.Server.ClientHandler;
using PointToPoint.Server.ClientHandler.Factories;

namespace PointToPointTests;

public class SystemTests
{
    private readonly MessageReceiver clientMessageReceiver = new();

    [Fact]
    public void MessagesBetweenClientAndServer()
    {
        // Arrange - start the server
        var port = 12345;

        var clientHandler = new ClientsHandler<TestClientHandler>(
            new NewtonsoftJsonPayloadSerializer(typeof(ClientToServerMessage).Assembly),
            new ActivatorClientHandlerFactory(),
            new ReflectionMessageRouterFactory());

        var tcpServer = new TcpServer(port);
        var tcpServerThread = new Thread(() => tcpServer.Run(clientHandler));
        tcpServerThread.Start();

        // Arrange - start the client
        var clientMessenger = new TcpMessenger("127.0.0.1", port,
            new NewtonsoftJsonPayloadSerializer(typeof(ClientToServerMessage).Assembly),
            new ReflectionMessageRouter(clientMessageReceiver),
            new SocketFactory());
        clientMessenger.Start();

        // Act - send message from client to server
        clientMessenger.Send(new ClientToServerMessage());

        Thread.Sleep(3000);

        // Shutdown
        tcpServer.Stop();
        clientHandler.Stop();
        clientMessenger.Stop();

        TestUtils.WaitFor(clientHandler.IsStopped);
        TestUtils.WaitFor(clientMessenger.IsStopped);

        // Assert
        Assert.Single(clientMessageReceiver.Messages.OfType<ServerToClientMessage>());
        Assert.Single(clientMessageReceiver.Messages.OfType<ServerToClientBroadcastMessage>());
        Assert.True(clientMessageReceiver.Messages.OfType<KeepAlive>().Count() > 1);
    }
}

public class MessageReceiver
{
    public List<object> Messages { get; } = [];

    public void HandleMessage(ServerToClientMessage message, IMessenger messenger) => Messages.Add(message);

    public void HandleMessage(ServerToClientBroadcastMessage message, IMessenger messenger) => Messages.Add(message);

    public void HandleMessage(KeepAlive message, IMessenger messenger) => Messages.Add(message);
}

public record ClientToServerMessage();
public record ServerToClientMessage();
public record ServerToClientBroadcastMessage();

public class TestClientHandler : IClientHandler
{
    public readonly List<object> receivedMessages = [];

    public void Init(IMessageSender messageSender, IMessageRouter messageRouter)
    {
        messageSender.SendMessage(new ServerToClientMessage(), this);
        messageSender.SendBroadcast(new ServerToClientBroadcastMessage());
    }

    public void Exit(Exception? _)
    {
        Assert.Single(receivedMessages.OfType<ClientToServerMessage>());
        Assert.True(receivedMessages.OfType<KeepAlive>().Count() > 1);
    }

    public void Update() { }

    public void HandleMessage(ClientToServerMessage message, IMessenger messenger)
    {
        receivedMessages.Add(message);
    }

    public void HandleMessage(KeepAlive message, IMessenger messenger)
    {
        receivedMessages.Add(message);
    }
}
