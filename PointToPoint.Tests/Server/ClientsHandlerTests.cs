using FakeItEasy;
using PointToPoint.MessageRouting.Factories;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using PointToPoint.Server;
using PointToPoint.Server.ClientHandler;
using PointToPoint.Server.ClientHandler.Factories;
using System.Net.Sockets;

namespace PointToPoint.Tests.Server;

public class ClientsHandlerTests
{
    [Fact]
    public void NewConnection()
    {
        // Arrange
        var clientHandlerForTest = new ClientHandlerForTest();
        var fakePayloadSerializer = A.Fake<IPayloadSerializer>();
        
        var fakeClientHandlerFactory = A.Fake<IClientHandlerFactory>();
        A.CallTo(() => fakeClientHandlerFactory.Create<ClientHandlerForTest>()).Returns(clientHandlerForTest);

        var fakemessageRouterFactory = A.Fake<IMessageRouterFactory>();

        // Act
        var clientsHandler = new ClientsHandler<ClientHandlerForTest>(fakePayloadSerializer, fakeClientHandlerFactory, fakemessageRouterFactory);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(1), clientsHandler.KeepAliveSendInterval);
        Assert.Equal(TcpMessenger.DefaultSocketOptions, clientsHandler.SocketOptions);

        Assert.False(clientHandlerForTest.InitCalled);
        Assert.False(clientHandlerForTest.ExitCalled);
        Assert.False(clientHandlerForTest.UpdateCalled);

        // Act
        var fakeSocket = CreateDefaultFakeSocket();
        clientsHandler.NewConnection(fakeSocket);

        // Assert
        Assert.True(clientHandlerForTest.InitCalled);
        Assert.False(clientHandlerForTest.ExitCalled);
        Assert.False(clientHandlerForTest.UpdateCalled);

        // Act
        clientsHandler.UpdateClients();

        // Assert
        Assert.True(clientHandlerForTest.InitCalled);
        Assert.False(clientHandlerForTest.ExitCalled);
        Assert.True(clientHandlerForTest.UpdateCalled);

        // Act
        clientsHandler.Stop();

        // Assert
        TestUtils.WaitFor(() => clientsHandler.IsStopped());
        Assert.False(clientHandlerForTest.ExitCalled);
    }

    [Fact]
    public void NewConnection_SendFailure_RemoveClientCallsExitAndStopsMessenger()
    {
        // Arrange
        var clientHandlerForTest = new ClientHandlerForTest();
        var fakePayloadSerializer = A.Fake<IPayloadSerializer>();
        A.CallTo(() => fakePayloadSerializer.MessageToPayload(A<object>._)).Returns(new byte[] { 1 });

        var fakeClientHandlerFactory = A.Fake<IClientHandlerFactory>();
        A.CallTo(() => fakeClientHandlerFactory.Create<ClientHandlerForTest>()).Returns(clientHandlerForTest);

        var fakeMessageRouterFactory = A.Fake<IMessageRouterFactory>();
        var clientsHandler = new ClientsHandler<ClientHandlerForTest>(fakePayloadSerializer, fakeClientHandlerFactory, fakeMessageRouterFactory);

        var fakeSocket = CreateDefaultFakeSocket();
        A.CallTo(() => fakeSocket.SendAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._)).Throws(new InvalidOperationException("fail"));
        clientsHandler.NewConnection(fakeSocket);

        // Assert
        TestUtils.WaitFor(() => clientHandlerForTest.ExitCalled);
        Assert.True(clientHandlerForTest.InitCalled);
        Assert.True(clientHandlerForTest.ExitCalled);
        TestUtils.WaitFor(clientsHandler.IsStopped);
    }

    [Fact]
    public void UpdateClients_DisconnectDuringUpdate_DoesNotThrowAndCleansUpClient()
    {
        // Arrange
        var clientHandlerForTest = new DisconnectingClientHandlerForTest();
        var fakePayloadSerializer = A.Fake<IPayloadSerializer>();

        var fakeClientHandlerFactory = A.Fake<IClientHandlerFactory>();
        A.CallTo(() => fakeClientHandlerFactory.Create<DisconnectingClientHandlerForTest>()).Returns(clientHandlerForTest);

        var fakeMessageRouterFactory = A.Fake<IMessageRouterFactory>();
        var clientsHandler = new ClientsHandler<DisconnectingClientHandlerForTest>(fakePayloadSerializer, fakeClientHandlerFactory, fakeMessageRouterFactory);

        var fakeSocket = CreateDefaultFakeSocket();
        clientsHandler.NewConnection(fakeSocket);

        // Act
        clientsHandler.UpdateClients();

        // Assert
        Assert.True(clientHandlerForTest.InitCalled);
        Assert.True(clientHandlerForTest.UpdateCalled);
        Assert.True(clientHandlerForTest.ExitCalled);
        TestUtils.WaitFor(clientsHandler.IsStopped);
    }

    private static ISocket CreateDefaultFakeSocket()
    {
        var fakeSocket = A.Fake<ISocket>();
        A.CallTo(() => fakeSocket.ReceiveAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._))
            .ReturnsLazily((byte[] _, int __, int ___, SocketFlags ____, CancellationToken token) => Task.Delay(Timeout.Infinite, token).ContinueWith(_ => 0, token));
        A.CallTo(() => fakeSocket.SendAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._))
            .ReturnsLazily((byte[] _, int __, int size, SocketFlags ____, CancellationToken _____) => Task.FromResult(size));

        return fakeSocket;
    }
}

public class ClientHandlerForTest : IClientHandler
{
    public bool ExitCalled { get; private set; } = false;
    public bool InitCalled { get; private set; } = false;
    public bool UpdateCalled { get; private set; } = false;

    public void Init(IClient client) => InitCalled = true;

    public void Exit(Exception? e) => ExitCalled = true;

    public void Update() => UpdateCalled = true;
}

public class DisconnectingClientHandlerForTest : IClientHandler
{
    private IClient? client;
    private bool disconnected;

    public bool ExitCalled { get; private set; } = false;
    public bool InitCalled { get; private set; } = false;
    public bool UpdateCalled { get; private set; } = false;

    public void Init(IClient client)
    {
        this.client = client;
        InitCalled = true;
    }

    public void Exit(Exception? e) => ExitCalled = true;

    public void Update()
    {
        UpdateCalled = true;
        if (!disconnected)
        {
            disconnected = true;
            client!.Disconnect();
        }
    }
}
