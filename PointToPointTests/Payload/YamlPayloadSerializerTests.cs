using PointToPoint.Payload;
using PointToPointProtocol;
using System.Text;

namespace PointToPointTests.Payload;

// Note: parameterless constructor needed for this paylaod serializer
public class YamlMessage
{
    public int Value { get; set; }
    public string? Text { get; set; }
}

public class YamlPayloadSerializerTests
{
    [Fact]
    public void SerializeDeserialize_CustomMessage()
    {
        var message = new YamlMessage() { Value = 10, Text = "text" };
        var serializer = new YamlPayloadSerializer(typeof(MyMessage).Assembly);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = (YamlMessage)serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message.Value, deserializedMessage.Value);
        Assert.Equal(message.Text, deserializedMessage.Text);
    }

    [Fact]
    public void SerializeDeserialize_KeepAlive()
    {
        var message = new KeepAlive();
        var serializer = new YamlPayloadSerializer(typeof(MyMessage).Assembly);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void MessageToPayload_NonProtocolMessage_Throws()
    {
        // Arrange
        var message = "message";
        var serializer = new YamlPayloadSerializer(typeof(MyMessage).Assembly);

        // Act
        Assert.Throws<ArgumentException>(() => serializer.MessageToPayload(message));
    }

    [Fact]
    public void PayloadToMessage_SeparatorNotFound_Throws()
    {
        // Arrange
        var bytes = Encoding.Unicode.GetBytes("datawithoutseparator");
        var serializer = new YamlPayloadSerializer(typeof(MyMessage).Assembly);

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
        var serializer = new YamlPayloadSerializer(protocolAssembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_NonProtocolType_Throws()
    {
        // Arrange
        var protocolAssembly = typeof(MyMessage).Assembly;
        var bytes = Encoding.Unicode.GetBytes($"{typeof(string)} ");
        var serializer = new YamlPayloadSerializer(protocolAssembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_NoYamlIncluded()
    {
        // Arrange
        var protocolNamespace = typeof(MyMessage).Namespace!;
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(MyMessage)} ");
        var serializer = new YamlPayloadSerializer(typeof(MyMessage).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }
}
