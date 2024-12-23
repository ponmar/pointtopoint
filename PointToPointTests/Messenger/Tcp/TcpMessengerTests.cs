using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using System.Net.Sockets;

namespace PointToPointTests.Messenger.Tcp;

public class TcpMessengerTests : IDisposable
{
    private readonly IPayloadSerializer fakePayloadSerializer = A.Fake<IPayloadSerializer>();
    private readonly IMessageRouter fakeMessageRouter = A.Fake<IMessageRouter>();
    private readonly ISocketFactory fakeSocketFactory = A.Fake<ISocketFactory>();
    private readonly ISocket fakeSocket = A.Fake<ISocket>();

    private TcpMessenger? messenger;

    public TcpMessengerTests()
    {
        A.CallTo(() => fakeSocketFactory.Create(A<AddressFamily>._)).Returns(fakeSocket);
    }

    public void Dispose()
    {
        if (messenger is not null)
        {
            messenger.Stop();
            TestUtils.WaitFor(() => messenger.IsStopped());
        }
    }

    [Fact]
    public void Constructor_DefaultSocketOptions_SocketOptionsSet()
    {
        // Arrange
        var socketOptions = TcpMessenger.DefaultSocketOptions;

        // Act
        messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            socketOptions);

        // Assert
        A.CallTo(fakeSocket).Where(x => x.Method.Name.Equals("set_NoDelay") && x.Arguments.First()!.Equals(socketOptions.NoDelay)).MustHaveHappenedOnceExactly();
        A.CallTo(fakeSocket).Where(x => x.Method.Name.Equals("set_ReceiveTimeout") && x.Arguments.First()!.Equals(socketOptions.ReceiveTimeout)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Start_CalledTwice_Throws()
    {
        // Arrange
        messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            TcpMessenger.DefaultSocketOptions);

        messenger.Start();

        // Act
        Assert.Throws<InvalidOperationException>(messenger.Start);
    }

    [Fact]
    public void Send_AllBytesWrittenToSocket()
    {
        // Arrange
        var message = new MyMessage(1337);
        var serializedPayload = new byte[] { 1 };
        var serializedPayloadLength = serializedPayload.Length;

        var keepAliveMessage = new KeepAlive();
        var serializedKeepAlivePayload = new byte[] { 1, 2 };
        var serializedKeepAlivePayloadLength = serializedKeepAlivePayload.Length;

        //var fakeSocket = A.Fake<ISocket>();
        //A.CallTo(() => fakeSocketFactory.Create(A<AddressFamily>._)).Returns(fakeSocket);

        A.CallTo(() => fakePayloadSerializer.MessageToPayload(message)).Returns(serializedPayload);
        A.CallTo(() => fakeSocket.Send(A<byte[]>._, 0, 4 + serializedPayloadLength, SocketFlags.None)).Returns(4 + serializedPayloadLength);

        A.CallTo(() => fakePayloadSerializer.MessageToPayload(keepAliveMessage)).Returns(serializedKeepAlivePayload);
        A.CallTo(() => fakeSocket.Send(A<byte[]>._, 0, 4 + serializedKeepAlivePayloadLength, SocketFlags.None)).Returns(4 + serializedKeepAlivePayloadLength);

        messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            TcpMessenger.DefaultSocketOptions);

        messenger.Start();

        // Act
        messenger.Send(message);

        // Assert
        TestUtils.WaitForAssert(() =>
            A.CallTo(() => fakeSocket.Send(A<byte[]>._, 0, 4 + serializedPayloadLength, SocketFlags.None)).MustHaveHappenedOnceExactly()
        );
    }
}

record MyMessage(int Value);
