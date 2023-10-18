using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Network
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
            IPAddress ipAddress = IPAddress.Any;

            if (networkInterface != string.Empty &&
                !IPAddress.TryParse(networkInterface, out ipAddress))
            {
                //log.Error("Invalid network interface");
                Stop();
                return;
            }

            TcpListener listener = new TcpListener(ipAddress, port);
            try
            {
                run = true;
                listener.Start();
            }
            catch (SocketException e)
            {
                //log.Error("Unable to open server socket", e);
                Stop();
            }

            while (run)
            {
                // TODO: how to timeout to stop earlier? listener.Server.Blocking = false?
                Socket socket = listener.AcceptSocket();
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
