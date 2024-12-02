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
    }
}