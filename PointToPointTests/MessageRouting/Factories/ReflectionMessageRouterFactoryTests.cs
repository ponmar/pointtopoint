using PointToPoint.MessageRouting.Factories;
using PointToPoint.MessageRouting;

namespace PointToPointTests.MessageRouting.Factories;

public class ReflectionMessageRouterFactoryTests
{
    [Fact]
    public void Create()
    {
        // Act
        var factory = new ReflectionMessageRouterFactory();
        var result = (ReflectionMessageRouter)factory.Create(new MessageHandlerForTest());

        // Assert
        Assert.NotNull(result);
    }
}
