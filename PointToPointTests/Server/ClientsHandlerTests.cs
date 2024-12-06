using FakeItEasy;
using PointToPoint.MessageRouting.Factories;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using PointToPoint.Server;
using PointToPoint.Server.ClientHandler;
using PointToPoint.Server.ClientHandler.Factories;

namespace PointToPointTests.Server;

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
        var fakeSocket = A.Fake<ISocket>();
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
        TestUtils.WaitFor(() => clientHandlerForTest.ExitCalled);
    }
}

public class ClientHandlerForTest : IClientHandler
{
    public bool ExitCalled { get; private set; } = false;
    public bool InitCalled { get; private set; } = false;
    public bool UpdateCalled { get; private set; } = false;

    public void Init(Client client) => InitCalled = true;

    public void Exit(Exception? e) => ExitCalled = true;

    public void Update() => UpdateCalled = true;
}
