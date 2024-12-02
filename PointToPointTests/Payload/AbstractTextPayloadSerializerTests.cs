using PointToPoint.Payload;
using System.Text;
using System.Reflection;

namespace PointToPointTests.Payload;

public class TextPayloadSerializerForTest : AbstractTextPayloadSerializer
{
    public TextPayloadSerializerForTest(Assembly messagesAssembly) : base(messagesAssembly)
    {
    }

    protected override object? DeserializeObject(string text, Type type)
    {
        return "Hello";
    }

    protected override string SerializeObject(object message)
    {
        return "Hello";
    }
}

public class AbstractTextPayloadSerializerTests
{
    [Fact]
    public void MessageToPayload_NonProtocolMessage_Throws()
    {
        // Arrange
        var message = "message";
        var serializer = new TextPayloadSerializerForTest(typeof(PayloadForTest).Assembly);

        // Act
        Assert.Throws<ArgumentException>(() => serializer.MessageToPayload(message));
    }

    [Fact]
    public void PayloadToMessage_SeparatorNotFound_Throws()
    {
        // Arrange
        var bytes = Encoding.Unicode.GetBytes("datawithoutseparator");
        var serializer = new TextPayloadSerializerForTest(typeof(PayloadForTest).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_UnknownType_Throws()
    {
        // Arrange
        var protocolAssembly = typeof(PayloadForTest).Assembly;
        var assemblyName = "Unknown";
        var bytes = Encoding.Unicode.GetBytes($"Some.Unknown.Type,{assemblyName} ");
        var serializer = new TextPayloadSerializerForTest(protocolAssembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_NonProtocolType_Throws()
    {
        // Arrange
        var protocolAssembly = typeof(PayloadForTest).Assembly;
        var bytes = Encoding.Unicode.GetBytes($"{typeof(string)} ");
        var serializer = new TextPayloadSerializerForTest(protocolAssembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }
}
