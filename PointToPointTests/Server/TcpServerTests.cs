using FakeItEasy;
using PointToPoint.Server;
using PointToPoint.Server.TcpListener;
using System.Net.Sockets;

namespace PointToPointTests.Server;

public class TcpServerTests
{
    [Fact]
    public void Run_NoInterfaceSpecified_StopWorks()
    {
        var fakeTcpListenerFactory = A.Fake<ITcpListenerFactory>();
        var fakeConnectionHandler = A.Fake<IConnectionHandler>();

        var tcpServer = new TcpServer(NetworkInterface.AnyIPv4, 12345, fakeTcpListenerFactory);
        var tcpServerThread = new Thread(() => tcpServer.Run(fakeConnectionHandler));
        tcpServerThread.Start();

        tcpServer.Stop();
        tcpServerThread.Join();
    }

    [Fact]
    public void Run_InvalidNetworkInterfaceNameSpecified_Throws()
    {
        var fakeTcpListenerFactory = A.Fake<ITcpListenerFactory>();
        var fakeConnectionHandler = A.Fake<IConnectionHandler>();

        var tcpServer = new TcpServer("invalidNetworkInterfaceName", 12345, fakeTcpListenerFactory);

        Assert.Throws<ArgumentException>(() => tcpServer.Run(fakeConnectionHandler));
    }

    [Fact]
    public void Run_AcceptSocketThrowsNonInterruptedSocketException_Rethrows()
    {
        var fakeTcpListenerFactory = A.Fake<ITcpListenerFactory>();
        var fakeTcpListener = A.Fake<ITcpListener>();
        var fakeConnectionHandler = A.Fake<IConnectionHandler>();

        A.CallTo(() => fakeTcpListenerFactory.Create(A<System.Net.IPAddress>._, A<int>._)).Returns(fakeTcpListener);
        A.CallTo(() => fakeTcpListener.AcceptSocket()).Throws(new SocketException((int)SocketError.ConnectionReset));

        var tcpServer = new TcpServer(NetworkInterface.AnyIPv4, 12345, fakeTcpListenerFactory);

        Assert.Throws<SocketException>(() => tcpServer.Run(fakeConnectionHandler));
    }
}
