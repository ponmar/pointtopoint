using System;
using System.Net;
using System.Net.Sockets;

namespace PointToPoint.Messenger.Tcp
{
    /// <summary>
    /// Socket interface to enable mocking for testing purposes.
    /// </summary>
    public interface ISocket
    {
        void Connect(EndPoint remoteEP);

        bool NoDelay { set; }

        TimeSpan ReceiveTimeout { set; }

        bool Connected { get; }

        void Shutdown(SocketShutdown how);

        void Close();

        void Dispose();

        int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags);

        int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags);
    }
}
