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

        public TcpServer(string networkInterface, int port, ITcpListenerFactory tcpListenerFactory)
        {
            this.networkInterface = networkInterface;
            this.port = port;
            this.tcpListenerFactory = tcpListenerFactory;
        }

        public void Run(IConnectionHandler clientHandler)
        {
            if (!IPAddress.TryParse(networkInterface, out var ipAddress))
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
