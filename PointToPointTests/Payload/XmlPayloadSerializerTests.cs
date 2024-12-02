using PointToPoint.Payload;
using PointToPoint.Protocol;
using System.Text;

namespace PointToPointTests.Payload;

public class XmlPayloadSerializerTests
{
    [Fact]
    public void SerializeDeserialize_PayloadRecord_NotSupported()
    {
        var message = new PayloadForTest(10, "text");
        var serializer = new XmlPayloadSerializer(typeof(PayloadForTest).Assembly);

        Assert.Throws<InvalidOperationException>(() => serializer.MessageToPayload(message));
    }

    [Fact]
    public void SerializeDeserialize_PayloadWithParameterlessConstructor()
    {
        // Arrange
        var message = new PayloadWithParameterlessConstructorForTest() { Value = 10, Text = "text" };
        var serializer = new XmlPayloadSerializer(typeof(PayloadWithParameterlessConstructorForTest).Assembly);

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
        var serializer = new XmlPayloadSerializer(typeof(PayloadWithParameterlessConstructorForTest).Assembly);

        // Act
        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        // Assert
        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void PayloadToMessage_NoXmlIncluded()
    {
        // Arrange
        var protocolNamespace = typeof(PayloadWithParameterlessConstructorForTest).Namespace!;
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(PayloadWithParameterlessConstructorForTest)} ");
        var serializer = new XmlPayloadSerializer(typeof(PayloadWithParameterlessConstructorForTest).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }
}
