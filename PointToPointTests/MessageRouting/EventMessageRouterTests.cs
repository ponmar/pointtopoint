using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger;

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
            var message = new MyMessage();
            var senderId = Guid.NewGuid();
            var messenger = A.Fake<IMessenger>();

            // Act - without listener
            messageRouter.RouteMessage(message, messenger);

            // Assert
            Assert.IsNull(messageInfo);

            // Act - with listener
            messageRouter.MessageReceived += (sender, e) => { messageInfo = e; };
            messageRouter.RouteMessage(message, messenger);

            // Assert
            Assert.IsNotNull(messageInfo);
            Assert.AreSame(message, messageInfo.Message);
            Assert.AreEqual(messenger, messageInfo.Messenger);
        }
    }

    public record MyMessage();
}