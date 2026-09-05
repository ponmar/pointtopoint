using PointToPoint.Payload;
using PointToPoint.Payload.MsJson;
using PointToPoint.Protocol;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PointToPointTests.Payload;

public class MsJsonPayloadSerializerTests
{
    [Fact]
    public void SerializeDeserialize_PayloadRecord()
    {
        // Arrange
        var message = new PayloadForTest(10, "text");
        var serializer = new MsJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

        // Act
        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        // Assert
        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void SerializeDeserialize_PayloadWithParameterlessConstructor()
    {
        // Arrange
        var message = new PayloadWithParameterlessConstructorForTest() { Value = 10, Text = "text" };
        var serializer = new MsJsonPayloadSerializer(typeof(PayloadWithParameterlessConstructorForTest).Assembly);

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
        var serializer = new MsJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

        // Act
        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        // Assert
        Assert.Equal(message, deserializedMessage);
    }

    [Fact]
    public void PayloadToMessage_NoJsonIncluded()
    {
        // Arrange
        var protocolNamespace = typeof(PayloadForTest).Namespace!;
        var assemblyName = "PointToPointTests";
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(PayloadForTest)},{assemblyName} ");
        var serializer = new MsJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void SerializeDeserialize_UsesSerializerOptions()
    {
        // Arrange
        var message = new MsJsonConverterPayload(10);
        var serializer = new MsJsonPayloadSerializer(
            typeof(MsJsonConverterPayload).Assembly,
            new JsonSerializerOptions { Converters = { new MsJsonConverterPayloadJsonConverter() } });

        // Act
        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        // Assert
        Assert.Equal(message, deserializedMessage);
    }
}

public record MsJsonConverterPayload(int Value);

public sealed class MsJsonConverterPayloadJsonConverter : JsonConverter<MsJsonConverterPayload>
{
    public override MsJsonConverterPayload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new MsJsonConverterPayload(int.Parse(reader.GetString()!, CultureInfo.InvariantCulture));
    }

    public override void Write(Utf8JsonWriter writer, MsJsonConverterPayload value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString(CultureInfo.InvariantCulture));
    }
}
