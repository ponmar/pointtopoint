using PointToPoint.Payload;
using PointToPoint.Payload.Yaml;
using PointToPoint.Protocol;
using System.Text;

namespace PointToPointTests.Payload;

public class YamlPayloadSerializerTests
{
    [Fact]
    public void SerializeDeserialize_CustomMessage()
    {
        var message = new PayloadWithParameterlessConstructorForTest() { Value = 10, Text = "text" };
        var serializer = new YamlPayloadSerializer(typeof(PayloadForTest).Assembly);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = (PayloadWithParameterlessConstructorForTest)serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message.Value, deserializedMessage.Value);
        Assert.Equal(message.Text, deserializedMessage.Text);
    }

    [Fact]
    public void SerializeDeserialize_KeepAlive()
    {
        var message = new KeepAlive();
        var serializer = new YamlPayloadSerializer(typeof(PayloadForTest).Assembly);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

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
