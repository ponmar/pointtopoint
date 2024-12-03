using PointToPoint.MessageRouting.Factories;
using PointToPoint.MessageRouting;

namespace PointToPointTests.MessageRouting.Factories;

public class QueueingReflectionMessageRouterFactoryTests
{
    [Fact]
    public void Create()
    {
        // Act
        var factory = new QueueingReflectionMessageRouterFactory();
        var result = (QueueingReflectionMessageRouter)factory.Create(new MessageHandlerForTest());

        // Assert
        Assert.NotNull(result);
    }
}
