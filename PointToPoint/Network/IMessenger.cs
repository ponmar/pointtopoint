using System;

namespace PointToPoint.Network
{
    // Sends and receives messages over TCP in format: <length (4 bytes)> <payload>
    public interface IMessenger
    {
        Guid Id { get; }
        void Start();
        void Close();
        void Send(object message);
    }
}
