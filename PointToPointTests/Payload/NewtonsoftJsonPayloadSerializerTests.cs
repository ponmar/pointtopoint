using PointToPoint.Payload;
using PointToPoint.Protocol;

namespace PointToPointTests.Payload;

[TestClass]
public class NewtonsoftJsonPayloadSerializerTests
{
    [TestMethod]
    public void SerializeDeserialize_CustomMessage()
    {
        var message = new MyMessage(10, "text");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Namespace!);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.AreEqual(message, deserializedMessage);
    }

    [TestMethod]
    public void SerializeDeserialize_KeepAlive()
    {
        var message = new KeepAlive();
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Namespace!);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.AreEqual(message, deserializedMessage);
    }
}

public record MyMessage(int Value, string Text);
