using FakeItEasy;
using PointToPoint.MessageRouting.Factories;
using PointToPoint.MessageRouting;
using PointToPoint.Server.ClientHandler;

namespace PointToPoint.Tests.MessageRouting.Factories;

public class EventMessageRouterFactoryTests
{
    [Fact]
    public void Create()
    {
        // Act
        var factory = new EventMessageRouterFactory();
        var result = (EventMessageRouter)factory.Create(A.Fake<IClientHandler>());

        // Assert
        Assert.NotNull(result);
    }
}
