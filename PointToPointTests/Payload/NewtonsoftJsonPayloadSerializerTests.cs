using PointToPoint.Payload;
using PointToPoint.Protocol;
using System.Text;

namespace PointToPointTests.Payload;

public class NewtonsoftJsonPayloadSerializerTests
{
    [Fact]
    public void SerializeDeserialize_CustomMessage()
    {
        var message = new MyMessage(10, "text");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Namespace!);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void SerializeDeserialize_KeepAlive()
    {
        var message = new KeepAlive();
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Namespace!);

        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void MessageToPayload_NonProtocolMessage_Throws()
    {
        // Arrange
        var message = new MyMessage(1, "");
        var serializer = new NewtonsoftJsonPayloadSerializer("Some.Namespace");

        // Act
        Assert.Throws<ArgumentException>(() => serializer.MessageToPayload(message));
    }

    [Fact]
    public void PayloadToMessage_SeparatorNotFound_Throws()
    {
        // Arrange
        var bytes = Encoding.Unicode.GetBytes("datawithoutseparator");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Namespace!);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_UnknownType_Throws()
    {
        // Arrange
        var protocolNamespace = typeof(MyMessage).Namespace!;
        var assemblyName = "Unknown";
        var bytes = Encoding.Unicode.GetBytes($"Some.Unknown.Type,{assemblyName} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(protocolNamespace);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_NonProtocolType_Throws()
    {
        // Arrange
        var protocolNamespace = typeof(MyMessage).Namespace!;
        var bytes = Encoding.Unicode.GetBytes($"{typeof(string)} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(protocolNamespace);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void PayloadToMessage_NoJsonIncluded()
    {
        // Arrange
        var protocolNamespace = typeof(MyMessage).Namespace!;
        var assemblyName = "PointToPointTests";
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(MyMessage)},{assemblyName} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(protocolNamespace);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }
}

public record MyMessage(int Value, string Text);
