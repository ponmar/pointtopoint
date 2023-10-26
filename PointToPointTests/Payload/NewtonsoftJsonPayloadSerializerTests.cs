using PointToPoint.Payload;
using PointToPoint.Protocol;
using System.Text;

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

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void MessageToPayload_NonProtocolMessage_Throws()
    {
        // Arrange
        var message = new MyMessage(1, "");
        var serializer = new NewtonsoftJsonPayloadSerializer("Some.Namespace");

        // Act
        serializer.MessageToPayload(message);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void PayloadToMessage_SeparatorNotFound_Throws()
    {
        // Arrange
        var bytes = Encoding.Unicode.GetBytes("datawithoutseparator");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(MyMessage).Namespace!);

        // Act
        serializer.PayloadToMessage(bytes, bytes.Length);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void PayloadToMessage_UnknownType_Throws()
    {
        // Arrange
        var protocolNamespace = typeof(MyMessage).Namespace!;
        var assemblyName = "Unknown";
        var bytes = Encoding.Unicode.GetBytes($"Some.Unknown.Type,{assemblyName} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(protocolNamespace);

        // Act
        serializer.PayloadToMessage(bytes, bytes.Length);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void PayloadToMessage_NonProtocolType_Throws()
    {
        // Arrange
        var protocolNamespace = typeof(MyMessage).Namespace!;
        var bytes = Encoding.Unicode.GetBytes($"{typeof(string)} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(protocolNamespace);

        // Act
        serializer.PayloadToMessage(bytes, bytes.Length);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void PayloadToMessage_NoJsonIncluded()
    {
        // Arrange
        var protocolNamespace = typeof(MyMessage).Namespace!;
        var assemblyName = "PointToPointTests";
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(MyMessage)},{assemblyName} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(protocolNamespace);

        // Act
        serializer.PayloadToMessage(bytes, bytes.Length);
    }
}

public record MyMessage(int Value, string Text);
