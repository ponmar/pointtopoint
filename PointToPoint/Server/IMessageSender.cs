using System;

namespace PointToPoint.Server
{
    public interface IMessageSender
    {
        void SendMessage(object message, IAppClientMessageHandler sender);
        void SendBroadcast(object message);
    }
}