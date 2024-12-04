﻿using FakeItEasy;
using PointToPoint.Server;

namespace PointToPointTests.Server;

public class TcpServertests
{
    [Fact]
    public void Run_NoInterfaceSpecified_StopWorks()
    {
        var fakeTcpListenerFactory = A.Fake<ITcpListenerFactory>();
        var fakeConnectionHandler = A.Fake<IConnectionHandler>();

        var tcpServer = new TcpServer(fakeTcpListenerFactory, 12345);
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

        var tcpServer = new TcpServer(fakeTcpListenerFactory, "invalidNetworkInterfaceName", 12345);

        Assert.Throws<ArgumentException>(() => tcpServer.Run(fakeConnectionHandler));
    }
}
