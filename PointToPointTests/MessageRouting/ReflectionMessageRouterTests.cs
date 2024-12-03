using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger;
using PointToPoint.Protocol;

namespace PointToPointTests.MessageRouting
{
    public class ReflectionMessageRouterTests
    {
        [Fact]
        public void RouteMessage_HandleMethodImplemented()
        {
            // Arrange
            var messageHandler = new MessageHandlerForTest();
            var messageRouter = new ReflectionMessageRouter(messageHandler);
            var message = new MyMessage();
            var messenger = A.Fake<IMessenger>();

            // Act
            messageRouter.RouteMessage(message, messenger);

            // Assert
            Assert.Single(messageHandler.Messages);
            Assert.Same(message, messageHandler.Messages.First());
        }

        [Fact]
        public void RouteMessage_HandleMethodImplementedAndExecutorSpecified()
        {
            // Arrange
            var messageHandler = new MessageHandlerForTest();
            var messageRouter = new ReflectionMessageRouter(messageHandler, (x) => x());
            var message = new MyMessage();
            var messenger = A.Fake<IMessenger>();

            // Act
            messageRouter.RouteMessage(message, messenger);

            // Assert
            Assert.Single(messageHandler.Messages);
            Assert.Same(message, messageHandler.Messages.First());
        }

        [Fact]
        public void RouteMessage_HandleMethodNotImplemented_ExceptionThrown()
        {
            // Arrange
            var messageHandler = new MessageHandlerForTest();
            var messageRouter = new ReflectionMessageRouter(messageHandler);

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
            Assert.Throws<NotImplementedException>(() => new ReflectionMessageRouter(messageHandler));
        }
    }

    public record UnknownMessage();

    public class MessageHandlerForTest
    {
        public List<object> Messages { get; } = [];

        public void HandleMessage(MyMessage message, IMessenger _) => Messages.Add(message);

        public void HandleMessage(KeepAlive message, IMessenger _) => Messages.Add(message);
    }

    public class MessageHandlerWithMissingKeepAliveHandleMethod
    {
    }
}