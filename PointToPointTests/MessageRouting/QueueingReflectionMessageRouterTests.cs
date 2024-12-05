using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger;

namespace PointToPointTests.MessageRouting
{
    public class QueueingReflectionMessageRouterTests
    {
        [Fact]
        public void RouteMessage_HandleMethodImplemented()
        {
            // Arrange
            var messageHandler = new MessageHandlerForTest();
            var messageRouter = new QueueingReflectionMessageRouter(messageHandler);
            var message = new MyMessage();
            var messenger = A.Fake<IMessenger>();

            // Act
            messageRouter.Update();

            // Assert
            Assert.Empty(messageHandler.Messages);

            // Act
            messageRouter.RouteMessage(message, messenger);

            // Assert
            Assert.Empty(messageHandler.Messages);

            // Act
            messageRouter.Update();

            // Assert
            Assert.Single(messageHandler.Messages);
            Assert.Same(message, messageHandler.Messages.First());
        }

        [Fact]
        public void RouteMessage_HandleMethodNotImplemented_ExceptionThrown()
        {
            // Arrange
            var messageHandler = new MessageHandlerForTest();
            var messageRouter = new QueueingReflectionMessageRouter(messageHandler);

            var message = new UnknownMessage();
            var messenger = A.Fake<IMessenger>();

            // Act
            Assert.Throws<NotImplementedException>(() => messageRouter.RouteMessage(message, messenger));
        }

        [Fact]
        public void RouteMessage_KeepAliveHandleMethodMissing()
        {
            // Arrange
            var messageHandler = new MessageHandlerWithMissingKeepAliveHandleMethod();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => new QueueingReflectionMessageRouter(messageHandler));
        }
    }
}