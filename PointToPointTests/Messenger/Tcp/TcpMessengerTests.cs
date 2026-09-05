using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger.Tcp;
using PointToPoint.Payload;
using PointToPoint.Protocol;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        A.CallTo(() => fakeSocket.ReceiveAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._))
            .ReturnsLazily((byte[] _, int __, int ___, SocketFlags ____, CancellationToken token) => Task.Delay(Timeout.Infinite, token).ContinueWith(_ => 0, token));
        A.CallTo(() => fakeSocket.SendAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._))
            .ReturnsLazily((byte[] _, int __, int size, SocketFlags ____, CancellationToken _____) => Task.FromResult(size));
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
    }

    [Fact]
    public void Constructor_NoSupportedServerAddresses_Throws()
    {
        // Arrange
        var fakeDnsResolver = A.Fake<IDnsResolver>();
        A.CallTo(() => fakeDnsResolver.GetHostEntry(A<string>._)).Returns(new IPHostEntry { AddressList = [] });

        // Act
        var ex = Assert.Throws<TargetInvocationException>(() => CreateTcpMessengerWithResolver(fakeDnsResolver));

        // Assert
        var inner = Assert.IsType<Exception>(ex.InnerException);
        Assert.Contains("No such server hostname available", inner.Message);
    }

    [Fact]
    public void Constructor_AllServerConnectionsFail_ThrowsWithLastConnectException()
    {
        // Arrange
        var connectException = new InvalidOperationException("connect-fail");
        var fakeDnsResolver = A.Fake<IDnsResolver>();
        A.CallTo(() => fakeDnsResolver.GetHostEntry(A<string>._)).Returns(new IPHostEntry { AddressList = [IPAddress.Loopback] });
        A.CallTo(() => fakeSocket.Connect(A<EndPoint>._)).Throws(connectException);

        // Act
        var ex = Assert.Throws<TargetInvocationException>(() => CreateTcpMessengerWithResolver(fakeDnsResolver));

        // Assert
        var inner = Assert.IsType<Exception>(ex.InnerException);
        Assert.Equal("Unable to connect to any server", inner.Message);
        Assert.Same(connectException, inner.InnerException);
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

        A.CallTo(() => fakePayloadSerializer.MessageToPayload(message)).Returns(serializedPayload);
        A.CallTo(() => fakeSocket.SendAsync(A<byte[]>._, 0, 4 + serializedPayloadLength, SocketFlags.None, A<CancellationToken>._))
            .Returns(Task.FromResult(4 + serializedPayloadLength));

        A.CallTo(() => fakePayloadSerializer.MessageToPayload(keepAliveMessage)).Returns(serializedKeepAlivePayload);
        A.CallTo(() => fakeSocket.SendAsync(A<byte[]>._, 0, 4 + serializedKeepAlivePayloadLength, SocketFlags.None, A<CancellationToken>._))
            .Returns(Task.FromResult(4 + serializedKeepAlivePayloadLength));
        A.CallTo(() => fakeSocket.ReceiveAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._))
            .ReturnsLazily((byte[] _, int __, int ___, SocketFlags ____, CancellationToken token) => Task.Delay(Timeout.Infinite, token).ContinueWith(_ => 0, token));

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
            A.CallTo(() => fakeSocket.SendAsync(A<byte[]>._, 0, 4 + serializedPayloadLength, SocketFlags.None, A<CancellationToken>._)).MustHaveHappenedOnceExactly(),
            TimeSpan.FromSeconds(10)
        );
    }

    [Fact]
    public void SendThreadFailure_RaisesDisconnectedOnceWithException()
    {
        // Arrange
        var disconnected = new ManualResetEventSlim();
        Exception? disconnectedException = null;
        var disconnectedCount = 0;
        var sendException = new InvalidOperationException("boom");

        A.CallTo(() => fakeSocket.Connected).Returns(true);
        A.CallTo(() => fakeSocket.ReceiveAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._))
            .ReturnsLazily((byte[] _, int __, int ___, SocketFlags ____, CancellationToken token) => Task.Delay(Timeout.Infinite, token).ContinueWith(_ => 0, token));
        A.CallTo(() => fakeSocket.SendAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._)).Throws(sendException);
        A.CallTo(() => fakePayloadSerializer.MessageToPayload(A<object>._)).Returns(new byte[] { 1 });

        messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            TcpMessenger.DefaultSocketOptions)
        {
            KeepAliveSendInterval = TimeSpan.Zero
        };

        messenger.Disconnected += (_, e) =>
        {
            disconnectedException = e;
            Interlocked.Increment(ref disconnectedCount);
            disconnected.Set();
        };

        // Act
        messenger.Start();

        // Assert
        Assert.True(disconnected.Wait(TimeSpan.FromSeconds(10)));
        TestUtils.WaitForAssert(() => Assert.Equal(1, Volatile.Read(ref disconnectedCount)), TimeSpan.FromSeconds(2));
        Assert.Same(sendException, disconnectedException);

        messenger.Stop();
        TestUtils.WaitForAssert(() =>
        {
            A.CallTo(() => fakeSocket.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocket.Dispose()).MustHaveHappenedOnceExactly();
        }, TimeSpan.FromSeconds(10));

        messenger = null;
    }

    [Fact]
    public void SendAndReceiveFailures_ConnectedAlreadyHandled_DisconnectedRaisedOnce()
    {
        // Arrange
        var disconnected = new ManualResetEventSlim();
        Exception? disconnectedException = null;
        var disconnectedCount = 0;
        var sendException = new InvalidOperationException("send-fail");
        var receiveException = new InvalidOperationException("receive-fail");

        A.CallTo(() => fakeSocket.Connected).Returns(true);
        A.CallTo(() => fakeSocket.ReceiveAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._)).Throws(receiveException);
        A.CallTo(() => fakeSocket.SendAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._)).Throws(sendException);
        A.CallTo(() => fakePayloadSerializer.MessageToPayload(A<object>._)).Returns(new byte[] { 1 });

        messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            TcpMessenger.DefaultSocketOptions)
        {
            KeepAliveSendInterval = TimeSpan.Zero
        };

        messenger.Disconnected += (_, e) =>
        {
            disconnectedException = e;
            Interlocked.Increment(ref disconnectedCount);
            disconnected.Set();
        };

        // Act
        messenger.Start();

        // Assert
        Assert.True(disconnected.Wait(TimeSpan.FromSeconds(10)));
        TestUtils.WaitForAssert(() => Assert.Equal(1, Volatile.Read(ref disconnectedCount)), TimeSpan.FromSeconds(2));
        Assert.Contains(disconnectedException, new[] { sendException, receiveException });

        messenger.Stop();
        TestUtils.WaitForAssert(() =>
        {
            A.CallTo(() => fakeSocket.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocket.Dispose()).MustHaveHappenedOnceExactly();
        }, TimeSpan.FromSeconds(10));

        messenger = null;
    }

    [Fact]
    public void ReceiveMessage_PayloadDeserializeThrows_RaisesDisconnected()
    {
        // Arrange
        var disconnected = new ManualResetEventSlim();
        Exception? disconnectedException = null;
        var deserializeException = new InvalidOperationException("deserialize-fail");
        var messageBytes = new byte[] { 0x2A };
        var lengthBytes = BitConverter.GetBytes(messageBytes.Length);
        var receiveCount = 0;

        A.CallTo(() => fakeSocket.Connected).Returns(true);
        A.CallTo(() => fakeSocket.ReceiveAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._))
            .ReturnsLazily((byte[] buffer, int offset, int size, SocketFlags _, CancellationToken token) =>
            {
                receiveCount++;
                if (receiveCount == 1)
                {
                    lengthBytes.CopyTo(buffer, offset);
                    return Task.FromResult(lengthBytes.Length);
                }

                if (receiveCount == 2)
                {
                    messageBytes.CopyTo(buffer, offset);
                    return Task.FromResult(messageBytes.Length);
                }

                return Task.Delay(Timeout.Infinite, token).ContinueWith(_ => 0, token);
            });

        A.CallTo(() => fakePayloadSerializer.PayloadToMessage(A<byte[]>._, messageBytes.Length)).Throws(deserializeException);
        A.CallTo(() => fakePayloadSerializer.MessageToPayload(A<object>._)).Returns(new byte[] { 1 });

        messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            TcpMessenger.DefaultSocketOptions)
        {
            KeepAliveSendInterval = TimeSpan.FromSeconds(10)
        };

        messenger.Disconnected += (_, e) =>
        {
            disconnectedException = e;
            disconnected.Set();
        };

        // Act
        messenger.Start();

        // Assert
        Assert.True(disconnected.Wait(TimeSpan.FromSeconds(10)));
        Assert.Same(deserializeException, disconnectedException);

        messenger.Stop();
        TestUtils.WaitForAssert(() =>
        {
            A.CallTo(() => fakeSocket.Close()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocket.Dispose()).MustHaveHappenedOnceExactly();
        }, TimeSpan.FromSeconds(10));

        messenger = null;
    }

    [Fact]
    public void Stop_ShutdownThrows_StillClosesAndDisposesSocket()
    {
        // Arrange
        A.CallTo(() => fakeSocket.Connected).Returns(true);
        A.CallTo(() => fakeSocket.Shutdown(SocketShutdown.Both)).Throws(new InvalidOperationException("boom"));
        A.CallTo(() => fakeSocket.ReceiveAsync(A<byte[]>._, A<int>._, A<int>._, SocketFlags.None, A<CancellationToken>._))
            .ReturnsLazily((byte[] _, int __, int ___, SocketFlags ____, CancellationToken token) => Task.Delay(Timeout.Infinite, token).ContinueWith(_ => 0, token));

        messenger = new TcpMessenger("localhost", 12345,
            fakePayloadSerializer,
            fakeMessageRouter,
            fakeSocketFactory,
            TcpMessenger.DefaultSocketOptions);

        // Act
        Assert.Throws<InvalidOperationException>(() => messenger.Stop());

        // Assert
        A.CallTo(() => fakeSocket.Close()).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeSocket.Dispose()).MustHaveHappenedOnceExactly();

        messenger = null;
    }

    private object CreateTcpMessengerWithResolver(IDnsResolver dnsResolver)
    {
        var constructor = typeof(TcpMessenger).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [
                typeof(string),
                typeof(int),
                typeof(IPayloadSerializer),
                typeof(IMessageRouter),
                typeof(ISocketFactory),
                typeof(SocketOptions),
                typeof(IDnsResolver)
            ],
            null);

        Assert.NotNull(constructor);
        return constructor!.Invoke(
            [
                "host-for-test",
                12345,
                fakePayloadSerializer,
                fakeMessageRouter,
                fakeSocketFactory,
                TcpMessenger.DefaultSocketOptions,
                dnsResolver
            ]);
    }
}

record MyMessage(int Value);
