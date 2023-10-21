using PointToPoint.MessageRouting;

namespace PointToPointTests.MessageRouting
{
    [TestClass]
    public class ReflectionMessageRouterTests
    {
        [TestMethod]
        public void RouteMessage_HandleMethodImplemented()
        {
            // Arrange
            var messageHandler = new MessageHandler();
            var messageRouter = new ReflectionMessageRouter
            {
                MessageHandler = messageHandler,
            };

            var message = new MyMessage();
            var senderId = Guid.NewGuid();

            // Act
            messageRouter.RouteMessage(message, senderId);

            // Assert
            Assert.AreEqual(1, messageHandler.Messages.Count);
            Assert.AreSame(message, messageHandler.Messages.First());
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void RouteMessage_HandleMethodNotImplemented_ExceptionThrown()
        {
            // Arrange
            var messageHandler = new MessageHandler();
            var messageRouter = new ReflectionMessageRouter
            {
                MessageHandler = messageHandler,
            };

            var message = new UnknownMessage();
            var senderId = Guid.NewGuid();

            // Act
            messageRouter.RouteMessage(message, senderId);
        }
    }

    public record UnknownMessage();

    public class MessageHandler
    {
        public List<object> Messages { get; } = new();

        public void HandleMessage(MyMessage message, Guid _)
        {
            Messages.Add(message);
        }
    }
}