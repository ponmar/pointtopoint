using PointToPoint.MessageRouting;

namespace PointToPointTests.MessageRouting
{
    [TestClass]
    public class EventMessageRouterTests
    {
        [TestMethod]
        public void RouteMessage()
        {
            // Arrange
            var messageRouter = new EventMessageRouter();
            MessageInfo? messageInfo = null;
            messageRouter.MessageReceived += (sender, e) => { messageInfo = e; };
            var message = new MyMessage();
            var senderId = Guid.NewGuid();

            // Act
            messageRouter.RouteMessage(message, senderId);

            // Assert
            Assert.IsNotNull(messageInfo);
            Assert.AreSame(message, messageInfo.Message);
            Assert.AreEqual(senderId, messageInfo.SenderId);
        }
    }

    public record MyMessage();
}