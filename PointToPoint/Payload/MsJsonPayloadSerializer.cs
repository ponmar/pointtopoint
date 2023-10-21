using System;
using System.Text;
using PointToPoint.Protocol;
using System.Text.Json;

namespace PointToPoint.Payload
{
    public class MsJsonPayloadSerializer : IPayloadSerializer
    {
        private static readonly Encoding PayloadTextEncoding = Encoding.Unicode;
        private const string IdPayloadSeparator = " ";

        private readonly string messagesNamespace;

        private readonly JsonSerializerOptions? serializerOptions;

        public MsJsonPayloadSerializer(string messagesNamespace, JsonSerializerOptions? serializerOptions = null)
        {
            this.messagesNamespace = messagesNamespace;
            this.serializerOptions = serializerOptions;
        }

        public byte[] MessageToPayload(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var messageType = message.GetType();
            var assemblyName = messageType.Assembly.FullName.Split(',')[0];
            var messageString = $"{messageType},{assemblyName}{IdPayloadSeparator}{json}";
            return SerializeString(messageString);
        }

        public object PayloadToMessage(byte[] payloadBytes, int length)
        {
            var payload = DeserializeString(payloadBytes, length);

            var separatorIndex = payload.IndexOf(IdPayloadSeparator);
            if (separatorIndex < 1)
            {
                throw new Exception("Message type/json separator not found");
            }
            if (separatorIndex == payload.Length - 1)
            {
                throw new Exception("Message json not found");
            }

            var jsonTypeString = payload.Substring(0, separatorIndex);
            var jsonType = Type.GetType(jsonTypeString);
            if (jsonType == null)
            {
                throw new Exception($"Unknown message type: {jsonTypeString}");
            }

            if (jsonType != typeof(KeepAlive) &&
                jsonType.Namespace != messagesNamespace)
            {
                throw new Exception($"Non protocol message type received: {jsonTypeString}");
            }


            var json = payload.Substring(separatorIndex + 1);

            var message = JsonSerializer.Deserialize(json, jsonType, serializerOptions);
            if (message is null)
            {
                throw new Exception("Deserialize returned null");
            }
            return message;
        }

        private static byte[] SerializeString(string data)
        {
            return PayloadTextEncoding.GetBytes(data);
        }

        private string DeserializeString(byte[] bytes, int length)
        {
            return PayloadTextEncoding.GetString(bytes, 0, length);
        }
    }
}
