using System;

namespace PointToPoint.Messenger
{
    // Sends and receives messages over TCP in format: <length (4 bytes)> <payload>
    public interface IMessenger
    {
        Guid Id { get; }
        void Start();
        void Close();

        // This method is thread safe
        void Send(object message);
    }
}
