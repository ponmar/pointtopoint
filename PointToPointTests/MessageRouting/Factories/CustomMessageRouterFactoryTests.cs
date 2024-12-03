using FakeItEasy;
using PointToPoint.MessageRouting;
using PointToPoint.MessageRouting.Factories;
using PointToPoint.Server.ClientHandler;

namespace PointToPointTests.MessageRouting.Factories;

public class CustomMessageRouterFactoryTests
{
    [Fact]
    public void Create()
    {
        // Act
        var factory = new CustomMessageRouterFactory((a, m) => { });
        var result = (CustomMessageRouter)factory.Create(A.Fake<IClientHandler>());

        // Assert
        Assert.NotNull(result);
    }
}
