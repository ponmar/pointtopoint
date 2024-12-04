using System;
using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Server
{
    public class TcpServer
    {
        private readonly ITcpListenerFactory tcpListenerFactory;
        private ITcpListener? tcpListener;

        private readonly string networkInterface;
        private readonly int port;

        private bool run = true;

        public TcpServer(ITcpListenerFactory tcpListenerFactory, int port) : this(tcpListenerFactory, string.Empty, port)
        {
        }

        public TcpServer(ITcpListenerFactory tcpListenerFactory, string networkInterface, int port)
        {
            this.tcpListenerFactory = tcpListenerFactory;
            this.networkInterface = networkInterface;
            this.port = port;
        }

        public void Run(IConnectionHandler clientHandler)
        {
            var ipAddress = IPAddress.Any;

            if (networkInterface != string.Empty &&
                !IPAddress.TryParse(networkInterface, out ipAddress))
            {
                throw new ArgumentException("Invalid network interface");
            }

            tcpListener = tcpListenerFactory.Create(ipAddress, port);
            tcpListener.Start();

            while (run)
            {
                try
                {
                    var socket = tcpListener.AcceptSocket();
                    clientHandler.NewConnection(socket);
                }
                catch (SocketException e)
                {
                    // Stop method triggers this error during normal shutdown
                    if (e.SocketErrorCode != SocketError.Interrupted)
                    {
                        throw;
                    }
                }
            }
        }

        // This is threadsafe
        public void Stop()
        {
            run = false;

            // Abort any ongoing blocking AcceptSocket
            tcpListener?.Stop();
        }
    }
}
