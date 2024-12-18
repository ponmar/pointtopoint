﻿using PointToPoint.MessageRouting;
using PointToPoint.MessageRouting.Factories;
using PointToPoint.Messenger;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Server;
using PointToPoint.Server.ClientHandler;
using PointToPoint.Server.ClientHandler.Factories;
using PointToPoint.Payload.NewtonsoftJson;
using PointToPoint.Protocol;
using PointToPoint.Server.TcpListener;

namespace PointToPointTests;

public class SystemTests
{
    private readonly MessageReceiver clientMessageReceiver = new();

    [Theory]
    [InlineData(NetworkInterface.AnyIPv4)]
    [InlineData(NetworkInterface.AnyIPv6)]
    public void MessagesBetweenClientAndServer(string serverNetworkInterface)
    {
        // Arrange - start the server
        var port = 12345;

        var clientsHandler = new ClientsHandler<TestClientHandler>(
            new NewtonsoftJsonPayloadSerializer(typeof(ClientToServerMessage).Assembly),
            new ActivatorClientHandlerFactory(),
            new ReflectionMessageRouterFactory());

        var tcpServer = new TcpServer(serverNetworkInterface, port, new TcpListenerFactory());
        var tcpServerThread = new Thread(() => tcpServer.Run(clientsHandler));
        tcpServerThread.Start();

        TestUtils.WaitFor(() => tcpServer.HasStarted);

        // Arrange - start the client
        var clientMessenger = new TcpMessenger("localhost", port,
            new NewtonsoftJsonPayloadSerializer(typeof(ClientToServerMessage).Assembly),
            new ReflectionMessageRouter(clientMessageReceiver),
            new SocketFactory(),
            TcpMessenger.DefaultSocketOptions);
        clientMessenger.Start();

        // Act - send message from client to server
        clientMessenger.Send(new ClientToServerMessage());

        Thread.Sleep(3000);

        clientsHandler.UpdateClients();

        // Shutdown
        tcpServer.Stop();
        tcpServerThread.Join();
        clientsHandler.Stop();
        clientMessenger.Stop();

        TestUtils.WaitFor(clientsHandler.IsStopped);
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

    public void Init(IClient client)
    {
        client.Messenger.Send(new ServerToClientMessage());
        client.MessageBroadcaster.SendBroadcast(new ServerToClientBroadcastMessage());
    }

    public void Exit(Exception? _)
    {
        Assert.Single(receivedMessages.OfType<ClientToServerMessage>());
        Assert.True(receivedMessages.OfType<KeepAlive>().Count() > 1);
    }

    public void Update()
    {
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
