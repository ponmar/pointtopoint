using PointToPoint.Payload;
using PointToPoint.Payload.NewtonsoftJson;
using PointToPoint.Protocol;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace PointToPointTests.Payload;

public class NewtonsoftJsonPayloadSerializerTests
{
    [Fact]
    public void SerializeDeserialize_PayloadRecord()
    {
        // Arrange
        var message = new PayloadForTest(10, "text");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

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
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(PayloadWithParameterlessConstructorForTest).Assembly);

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
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

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
        var bytes = Encoding.Unicode.GetBytes($"{protocolNamespace}.{nameof(PayloadForTest)} ");
        var serializer = new NewtonsoftJsonPayloadSerializer(typeof(PayloadForTest).Assembly);

        // Act
        Assert.Throws<PayloadDeserializeException>(() => serializer.PayloadToMessage(bytes, bytes.Length));
    }

    [Fact]
    public void SerializeDeserialize_UsesSerializerSettings()
    {
        // Arrange
        var message = new NewtonsoftJsonConverterPayload(10);
        var serializer = new NewtonsoftJsonPayloadSerializer(
            typeof(NewtonsoftJsonConverterPayload).Assembly,
            new JsonSerializerSettings { Converters = { new NewtonsoftJsonConverterPayloadJsonConverter() } });

        // Act
        var payload = serializer.MessageToPayload(message);
        var deserializedMessage = serializer.PayloadToMessage(payload, payload.Length);

        // Assert
        Assert.Equal(message, deserializedMessage);
    }
}

public record NewtonsoftJsonConverterPayload(int Value);

public sealed class NewtonsoftJsonConverterPayloadJsonConverter : JsonConverter<NewtonsoftJsonConverterPayload>
{
    public override void WriteJson(JsonWriter writer, NewtonsoftJsonConverterPayload? value, JsonSerializer serializer)
    {
        writer.WriteValue(value!.Value.ToString(CultureInfo.InvariantCulture));
    }

    public override NewtonsoftJsonConverterPayload ReadJson(JsonReader reader, Type objectType, NewtonsoftJsonConverterPayload? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new NewtonsoftJsonConverterPayload(int.Parse(reader.Value!.ToString()!, CultureInfo.InvariantCulture));
    }

    public override bool CanRead => true;
}
