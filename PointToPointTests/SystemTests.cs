using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using PointToPoint.Server;

namespace PointToPointTests;

[TestClass]
public class SystemTests
{
    private readonly List<object> clientReceivedMessages = new();

    [TestMethod]
    public void MessagesBetweenClientAndServer()
    {
        // Arrange - start the server
        var port = 12345;
        var keepAliveInterval = TimeSpan.FromSeconds(1);

        var clientHandler = new ClientsHandler(
            new NewtonsoftJsonPayloadSerializer(typeof(ClientToServerMessage).Namespace!),
            typeof(TestClientHandler),
            keepAliveInterval);

        var tcpServer = new TcpServer("127.0.0.1", port);
        var tcpServerThread = new Thread(() => tcpServer.Run(clientHandler));
        tcpServerThread.Start();

        // Arrange - start the client
        var clientMessenger = new TcpMessenger("127.0.0.1", port,
            new NewtonsoftJsonPayloadSerializer(typeof(ClientToServerMessage).Namespace!),
            new ReflectionMessageRouter(this),
            new SocketFactory(),
            keepAliveInterval);
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
        Assert.AreEqual(1, clientReceivedMessages.OfType<ServerToClientMessage>().Count());
        Assert.AreEqual(1, clientReceivedMessages.OfType<ServerToClientBroadcastMessage>().Count());
        Assert.IsTrue(clientReceivedMessages.OfType<KeepAlive>().Count() > 1);
    }

    public void HandleMessage(ServerToClientMessage message, IMessenger messenger)
    {
        clientReceivedMessages.Add(message);
    }

    public void HandleMessage(ServerToClientBroadcastMessage message, IMessenger messenger)
    {
        clientReceivedMessages.Add(message);
    }

    public void HandleMessage(KeepAlive message, IMessenger messenger)
    {
        clientReceivedMessages.Add(message);
    }
}

public record ClientToServerMessage();
public record ServerToClientMessage();
public record ServerToClientBroadcastMessage();

public class TestClientHandler : IClientHandler, IDisposable
{
    public readonly List<object> receivedMessages = new();

    public void Init(IMessageSender messageSender)
    {
        messageSender.SendMessage(new ServerToClientMessage(), this);
        messageSender.SendBroadcast(new ServerToClientBroadcastMessage());
    }

    void IDisposable.Dispose()
    {
        Assert.AreEqual(1, receivedMessages.OfType<ClientToServerMessage>().Count());
        Assert.IsTrue(receivedMessages.OfType<KeepAlive>().Count() > 1);
    }

    public void HandleMessage(ClientToServerMessage message, IMessenger messenger)
    {
        receivedMessages.Add(message);
    }

    public void HandleMessage(KeepAlive message, IMessenger messenger)
    {
        receivedMessages.Add(message);
    }
}
