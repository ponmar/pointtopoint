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
        var message = new PayloadForTest(10, "text");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void SerializeDeserialize_KeepAlive()
    {
        var message = new KeepAlive();
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void PayloadToMessage_NoJsonIncluded()
    {
        // Arrange
        var protocolNamespace = typeof(PayloadForTest).Namespace!;
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(PayloadForTest)} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }
}

