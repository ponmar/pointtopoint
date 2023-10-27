using PointToPoint.Messenger.Tcp;
using System;
using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Server
{
    public class TcpServer
    {
        private readonly string networkInterface;
        private readonly int port;

        private bool run = false;

        public TcpServer(int port) : this(string.Empty, port)
        {
        }

        public TcpServer(string networkInterface, int port)
        {
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

            // TODO: use an interface for TcpListener and a factory to be able to mock it during test
            var listener = new TcpListener(ipAddress, port);

            // May throw SocketException
            listener.Start();

            run = true;
            while (run)
            {
                // TODO: how to timeout to stop earlier? listener.Server.Blocking = false?
                var socket = new SocketWrapper(listener.AcceptSocket());
                clientHandler.NewConnection(socket);
            }
        }

        public void Stop()
        {
            if (run)
            {
                //log.Info("Stopping server");
                run = false;
            }
        }
    }
}
