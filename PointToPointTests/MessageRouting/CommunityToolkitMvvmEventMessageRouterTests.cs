using CommunityToolkit.Mvvm.Messaging;
using FakeItEasy;
using PointToPoint.MessageRouting.CommunityToolkitMvvm;

namespace PointToPointTests.MessageRouting
{
    public class CommunityToolkitMvvmEventMessageRouterTests : IDisposable
    {
        public void Dispose()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        [Fact]
        public void RouteMessage()
        {
            // Arrange
            var routedMessages = new List<MyMessage>();
            var eventChannel = 1337;
            var messageRouter = new CommunityToolkitMvvmEventMessageRouter(WeakReferenceMessenger.Default, eventChannel);
            var messenger = A.Fake<PointToPoint.Messenger.IMessenger>();

            WeakReferenceMessenger.Default.Register<MyMessage, int>(this, eventChannel, (r, m) => routedMessages.Add(m));

            // Act
            messageRouter.RouteMessage(new MyMessage(), messenger);

            // Assert
            Assert.Single(routedMessages);
        }

        [Fact]
        public void RouteMessage_WithExecutor()
        {
            // Arrange
            var routedMessages = new List<MyMessage>();
            var eventChannel = 1337;
            var messageRouter = new CommunityToolkitMvvmEventMessageRouter(WeakReferenceMessenger.Default, eventChannel, (a) => a());
            var messenger = A.Fake<PointToPoint.Messenger.IMessenger>();

            WeakReferenceMessenger.Default.Register<MyMessage, int>(this, eventChannel, (r, m) => routedMessages.Add(m));

            // Act
            messageRouter.RouteMessage(new MyMessage(), messenger);

            // Assert
            Assert.Single(routedMessages);
        }

        [Fact]
        public void Update_NotUsed_Coverage()
        {
            // Arrange
            var eventChannel = 1337;
            var messageRouter = new CommunityToolkitMvvmEventMessageRouter(WeakReferenceMessenger.Default, eventChannel);

            // Act
            messageRouter.Update();
        }

        [Fact]
        public void Constructor_SenderWithoutPublicSend_Throws()
        {
            // Arrange
            var sender = new ExplicitInterfaceOnlyMessenger();
            var eventChannel = 1337;

            // Act
            var exception = Assert.Throws<ArgumentException>(() => new CommunityToolkitMvvmEventMessageRouter(sender, eventChannel));

            // Assert
            Assert.Contains("Could not find Send method", exception.Message);
        }
    }

    internal sealed class ExplicitInterfaceOnlyMessenger : CommunityToolkit.Mvvm.Messaging.IMessenger
    {
        bool CommunityToolkit.Mvvm.Messaging.IMessenger.IsRegistered<TMessage, TToken>(object recipient, TToken token)
            => throw new NotImplementedException();

        void CommunityToolkit.Mvvm.Messaging.IMessenger.Register<TRecipient, TMessage, TToken>(TRecipient recipient, TToken token, MessageHandler<TRecipient, TMessage> handler)
            => throw new NotImplementedException();

        void CommunityToolkit.Mvvm.Messaging.IMessenger.UnregisterAll(object recipient)
            => throw new NotImplementedException();

        void CommunityToolkit.Mvvm.Messaging.IMessenger.UnregisterAll<TToken>(object recipient, TToken token)
            => throw new NotImplementedException();

        void CommunityToolkit.Mvvm.Messaging.IMessenger.Unregister<TMessage, TToken>(object recipient, TToken token)
            => throw new NotImplementedException();

        TMessage CommunityToolkit.Mvvm.Messaging.IMessenger.Send<TMessage, TToken>(TMessage message, TToken token)
            => throw new NotImplementedException();

        void CommunityToolkit.Mvvm.Messaging.IMessenger.Cleanup()
            => throw new NotImplementedException();

        void CommunityToolkit.Mvvm.Messaging.IMessenger.Reset()
            => throw new NotImplementedException();
    }
}