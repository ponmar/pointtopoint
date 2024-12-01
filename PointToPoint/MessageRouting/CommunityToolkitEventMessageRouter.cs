﻿using System;
using System.Reflection;
using IMessenger = PointToPoint.Messenger.IMessenger;

namespace PointToPoint.MessageRouting
{
    public class CommunityToolkitEventMessageRouter : IMessageRouter
    {
        private readonly CommunityToolkit.Mvvm.Messaging.IMessenger sender;
        private readonly int channel;
        private readonly Action<Action>? executor;

        private readonly MethodInfo sendMethod;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">WeakReferenceMessenger or StrongReferenceMessenger</param>
        /// <param name="channel">The event channel</param>
        /// <param name="executor">Can be used to route message on the UI thread in a WPF application by setting executor:Application.Current.Dispatcher.Invoke</param>
        public CommunityToolkitEventMessageRouter(CommunityToolkit.Mvvm.Messaging.IMessenger sender, int channel, Action<Action>? executor = null)
        {
            this.channel = channel;
            this.sender = sender;
            this.executor = executor;

            sendMethod = sender.GetType().GetMethod("Send");
            if (sendMethod is null)
            {
                throw new ArgumentException($"Could not find Send method on specified sender");
            }
        }

        public void RouteMessage(object message, IMessenger messenger)
        {
            var genericSendMethod = sendMethod.MakeGenericMethod(message.GetType(), typeof(int));
            var parameters = new object[] { message, channel };
            if (executor is null)
            {
                genericSendMethod.Invoke(sender, parameters);
            }
            else
            {
                executor.Invoke(() => genericSendMethod.Invoke(sender, parameters));
            }
        }
    }
}