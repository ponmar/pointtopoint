using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger;

namespace PointToPointTests.MessageRouting
{
    public class EventMessageRouterTests
    {
        [Fact]
        public void RouteMessage()
        {
            // Arrange
            var messageRouter = new EventMessageRouter();
            MessageInfo? messageInfo = null;
            var message = new MyMessage();
            var senderId = Guid.NewGuid();
            var messenger = A.Fake<IMessenger>();

            // Act - without listener
            messageRouter.RouteMessage(message, messenger);

            // Assert
            Assert.Null(messageInfo);

            // Act - with listener
            messageRouter.MessageReceived += (sender, e) => { messageInfo = e; };
            messageRouter.RouteMessage(message, messenger);

            // Assert
            Assert.NotNull(messageInfo);
            Assert.Same(message, messageInfo.Message);
            Assert.Equal(messenger, messageInfo.Messenger);
        }
    }

    public record MyMessage();
}