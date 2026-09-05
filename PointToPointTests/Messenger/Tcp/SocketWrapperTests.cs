using PointToPoint.Messenger.Tcp;
using System.Net;
using System.Net.Sockets;

namespace PointToPointTests.Messenger.Tcp;

public class SocketWrapperTests
{
    [Fact]
    public void Send_WritesBytesToPeerSocket()
    {
        var (clientSocket, serverSocket, listenerSocket) = CreateConnectedSockets();
        try
        {
            var payload = new byte[] { 1, 2, 3, 4 };
            var receiveBuffer = new byte[payload.Length];

            var sentBytes = clientSocket.Send(payload, 0, payload.Length, SocketFlags.None);
            var receivedBytes = serverSocket.Receive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None);

            Assert.Equal(payload.Length, sentBytes);
            Assert.Equal(payload.Length, receivedBytes);
            Assert.Equal(payload, receiveBuffer);
        }
        finally
        {
            clientSocket.Dispose();
            serverSocket.Dispose();
            listenerSocket.Dispose();
        }
    }

    [Fact]
    public void Receive_ReadsBytesFromPeerSocket()
    {
        var (clientSocket, serverSocket, listenerSocket) = CreateConnectedSockets();
        try
        {
            var payload = new byte[] { 9, 8, 7, 6 };
            var receiveBuffer = new byte[payload.Length];

            var sentBytes = serverSocket.Send(payload, 0, payload.Length, SocketFlags.None);
            var receivedBytes = clientSocket.Receive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None);

            Assert.Equal(payload.Length, sentBytes);
            Assert.Equal(payload.Length, receivedBytes);
            Assert.Equal(payload, receiveBuffer);
        }
        finally
        {
            clientSocket.Dispose();
            serverSocket.Dispose();
            listenerSocket.Dispose();
        }
    }

    private static (ISocket ClientSocket, Socket ServerSocket, Socket ListenerSocket) CreateConnectedSockets()
    {
        var listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listenerSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        listenerSocket.Listen(1);

        var port = ((IPEndPoint)listenerSocket.LocalEndPoint!).Port;
        var clientSocket = new SocketFactory().Create(AddressFamily.InterNetwork);
        clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, port));

        var serverSocket = listenerSocket.Accept();
        serverSocket.ReceiveTimeout = 5000;
        serverSocket.SendTimeout = 5000;

        return (clientSocket, serverSocket, listenerSocket);
    }
}
