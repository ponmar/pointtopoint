using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.Messenger;

namespace PointToPointTests.MessageRouting
{
    public class CustomMessageRouterTests
    {
        [Fact]
        public void RouteMessage()
        {
            // Arrange
            var routedMessages = new List<(object, IMessenger)>();
            var messageRouter = new CustomMessageRouter((message, messenger) => routedMessages.Add((message, messenger)));
            var messenger = A.Fake<IMessenger>();

            // Act
            messageRouter.RouteMessage(new MyMessage(), messenger);

            // Assert
            Assert.Single(routedMessages);
            Assert.Equal(typeof(MyMessage), routedMessages.First().Item1.GetType());
            Assert.Equal(messenger, routedMessages.First().Item2);
        }

        [Fact]
        public void RouteMessage_WithExecutor()
        {
            // Arrange
            var routedMessages = new List<(object, IMessenger)>();
            var messageRouter = new CustomMessageRouter((message, messenger) => routedMessages.Add((message, messenger)), (a) => a());
            var messenger = A.Fake<IMessenger>();

            // Act
            messageRouter.RouteMessage(new MyMessage(), messenger);

            // Assert
            Assert.Single(routedMessages);
            Assert.Equal(typeof(MyMessage), routedMessages.First().Item1.GetType());
            Assert.Equal(messenger, routedMessages.First().Item2);
        }
    }
}