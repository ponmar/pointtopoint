using System;
using System.Net;

namespace PointToPoint.Server
{
    public class TcpServer
    {
        private readonly ITcpListenerFactory tcpListenerFactory;
        private readonly string networkInterface;
        private readonly int port;

        private bool run = false;

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

            var listener = tcpListenerFactory.Create(ipAddress, port);
            listener.Start();

            run = true;
            while (run)
            {
                // TODO: how to timeout to stop earlier? listener.Server.Blocking = false?
                var socket = listener.AcceptSocket();
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
