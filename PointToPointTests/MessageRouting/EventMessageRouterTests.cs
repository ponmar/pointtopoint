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
            messageRouter.MessageReceived += (sender, e) => { messageInfo = e; };
            var message = new MyMessage();
            var senderId = Guid.NewGuid();
            var messenger = A.Fake<IMessenger>();

            // Act
            messageRouter.RouteMessage(message, messenger);

            // Assert
            Assert.IsNotNull(messageInfo);
            Assert.AreSame(message, messageInfo.Message);
            Assert.AreEqual(messenger, messageInfo.Messenger);
        }
    }

    public record MyMessage();
}