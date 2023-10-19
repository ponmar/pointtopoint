using System;

namespace PointToPoint.Network
{
    public interface IMessageSender
    {
        void SendMessage(object message, Guid receiverId);
        void SendBroadcast(object message);
    }
}