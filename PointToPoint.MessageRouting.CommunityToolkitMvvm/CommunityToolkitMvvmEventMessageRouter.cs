using System;
using System.Reflection;
using PointToPoint.Messenger;

namespace PointToPoint.MessageRouting.CommunityToolkitMvvm
{
    public class CommunityToolkitMvvmEventMessageRouter : IMessageRouter
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
        /// <param name="executor">Can be used to route message to the UI thread (in WPF: Application.Current.Dispatcher.Invoke, in Avalonia: Avalonia.Threading.Dispatcher.UIThread.Invoke)</param>
        public CommunityToolkitMvvmEventMessageRouter(CommunityToolkit.Mvvm.Messaging.IMessenger sender, int channel, Action<Action>? executor = null)
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

        public void Update() { }
    }
}