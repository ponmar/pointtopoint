﻿using PointToPoint.Server;
using PointToPoint.Protocol;
using Protocol;

namespace Server;

public class MessageHandler
{
    private readonly IMessageSender messageSender;

    public MessageHandler(IMessageSender messageSender)
    {
        this.messageSender = messageSender;
    }

    public void HandleMessage(PublishText message, Guid senderId)
    {
        Console.WriteLine($"Forwarding message '{message.Message}' to all.");
        messageSender.SendBroadcast(new Text(message.Message, DateTime.Now));

        switch (message.Message.ToLower())
        {
            case "hi":
            case "hello":
                Console.WriteLine($"Sending greeting to {senderId}.");
                messageSender.SendMessage(new Text($"And {message.Message} to you! /Server", DateTime.Now), senderId);
                break;
        }
    }

    public void HandleMessage(KeepAlive message, Guid senderId)
    {
    }
}