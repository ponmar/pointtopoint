using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using System.Net.Sockets;

namespace PointToPointTests.Messenger.Tcp;

public class TcpMessengerTests
{
    [Fact]
    public void Start_CalledTwice_Throws()
    {
        // Arrange
        var fakePayloadSerializer = A.Fake<IPayloadSerializer>();
        var fakeMessageRouter = A.Fake<IMessageRouter>();
        var fakeSocketFactory = A.Fake<ISocketFactory>();

        var messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            TimeSpan.FromSeconds(10));

        messenger.Start();

        // Act
        Assert.Throws<InvalidOperationException>(messenger.Start);
    }

    [Fact]
    public void Send_AllBytesWrittenToSocket()
    {
        // Arrange
        var message = new MyMessage(1337);
        var serializedPayload = new byte[] { 1, 2, 3 };
        var serializedPayloadLength = serializedPayload.Length;

        var fakePayloadSerializer = A.Fake<IPayloadSerializer>();
        var fakeMessageRouter = A.Fake<IMessageRouter>();
        var fakeSocketFactory = A.Fake<ISocketFactory>();
        var fakeSocket = A.Fake<ISocket>();

        A.CallTo(() => fakePayloadSerializer.MessageToPayload(message)).Returns(serializedPayload);
        A.CallTo(() => fakeSocketFactory.Create(A<AddressFamily>._)).Returns(fakeSocket);
        A.CallTo(() => fakeSocket.Send(A<byte[]>._, 0, 4 + serializedPayloadLength, SocketFlags.None)).Returns(4 + serializedPayloadLength);

        var messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            TimeSpan.FromSeconds(10));

        messenger.Start();

        // Act
        messenger.Send(message);
        Thread.Sleep(1000);

        // Assert
        A.CallTo(() => fakeSocket.Send(A<byte[]>._, 0, 4 + serializedPayloadLength, SocketFlags.None)).MustHaveHappenedOnceExactly();

        messenger.Stop();
    }
}

record MyMessage(int Value);
