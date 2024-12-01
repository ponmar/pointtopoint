using PointToPoint.Payload;
using PointToPoint.Payload.NewtonsoftJson;
using PointToPoint.Protocol;
using System.Text;

namespace PointToPointTests.Payload;

public class NewtonsoftJsonPayloadSerializerTests
{
    [Fact]
    public void SerializeDeserialize_CustomMessage()
    {
        var message = new MyMessage(10, "text");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Assembly);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void SerializeDeserialize_KeepAlive()
    {
        var message = new KeepAlive();
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Assembly);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void MessageToPayload_NonProtocolMessage_Throws()
    {
        // Arrange
        var message = "message";
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Assembly);

        // Act
        Assert.Throws<ArgumentException>(() => serializer.MessageToPayload(message));
    }

    [Fact]
    public void PayloadToMessage_SeparatorNotFound_Throws()
    {
        // Arrange
        var bytes = Encoding.Unicode.GetBytes("datawithoutseparator");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_UnknownType_Throws()
    {
        // Arrange
        var protocolAssembly = typeof(MyMessage).Assembly;
        var assemblyName = "Unknown";
        var bytes = Encoding.Unicode.GetBytes($"Some.Unknown.Type,{assemblyName} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(protocolAssembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_NonProtocolType_Throws()
    {
        // Arrange
        var protocolAssembly = typeof(MyMessage).Assembly;
        var bytes = Encoding.Unicode.GetBytes($"{typeof(string)} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(protocolAssembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_NoJsonIncluded()
    {
        // Arrange
        var protocolNamespace = typeof(MyMessage).Namespace!;
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(MyMessage)} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }
}

public record MyMessage(int Value, string Text);
