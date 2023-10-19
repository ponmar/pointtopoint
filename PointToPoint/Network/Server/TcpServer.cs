using System;
using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Network.Server
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

        public void Run(IClientHandler clientHandler)
        {
            var ipAddress = IPAddress.Any;

            if (networkInterface != string.Empty &&
                !IPAddress.TryParse(networkInterface, out ipAddress))
            {
                throw new Exception("Invalid network interface");
            }

            var listener = new TcpListener(ipAddress, port);

            // May throw SocketException
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
