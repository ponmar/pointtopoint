using PointToPoint.Payload;
using PointToPoint.Payload.Yaml;
using PointToPoint.Protocol;
using System.Text;
using YamlDotNet.Core;

namespace PointToPointTests.Payload;

public class YamlPayloadSerializerTests
{
    [Fact]
    public void SerializeDeserialize_PayloadRecord_NotSupported()
    {
        // Arrange
        var message = new PayloadForTest(10, "text");
        var serializer = new YamlPayloadSerializer(typeof(PayloadForTest).Assembly);

        // Act
        var payload = serializer.MessageToPayload(message);

        // Assert
        Assert.Throws<YamlException>(() => serializer.PayloadToMessage(payload, payload.Length));
    }

    [Fact]
    public void SerializeDeserialize_PayloadWithParameterlessConstructor()
    {
        // Arrange
        var message = new PayloadWithParameterlessConstructorForTest() { Value = 10, Text = "text" };
        var serializer = new YamlPayloadSerializer(typeof(PayloadWithParameterlessConstructorForTest).Assembly);

        // Act
        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = (PayloadWithParameterlessConstructorForTest)serializer.PayloadToMessage(payload, payload.Length);

        // Assert
        Assert.Equal(message.Value, deserializedMessage.Value);
        Assert.Equal(message.Text, deserializedMessage.Text);
    }

    [Fact]
    public void SerializeDeserialize_KeepAlive()
    {
        // Arrange
        var message = new KeepAlive();
        var serializer = new YamlPayloadSerializer(typeof(PayloadForTest).Assembly);

        // Act
        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        // Assert
        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void PayloadToMessage_NoYamlIncluded()
    {
        // Arrange
        var protocolNamespace = typeof(PayloadForTest).Namespace!;
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(PayloadForTest)} ");
        var serializer = new YamlPayloadSerializer(typeof(PayloadForTest).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }
}
