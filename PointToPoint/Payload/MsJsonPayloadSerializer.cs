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
            var messageType = message.GetType();
            if (!TypeIsProtocolMessage(messageType))
            {
                throw new ArgumentException($"Non protocol message can not be sent: {messageType}");
            }
            var json = JsonSerializer.Serialize(message);
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
                throw new PayloadDeserializeException("Message type/json separator not found");
            }

            var jsonTypeString = payload.Substring(0, separatorIndex);
            var jsonType = Type.GetType(jsonTypeString);
            if (jsonType == null)
            {
                throw new PayloadDeserializeException($"Unknown message type: {jsonTypeString}");
            }

            if (!TypeIsProtocolMessage(jsonType))
            {
                throw new PayloadDeserializeException($"Non protocol message type received: {jsonTypeString}");
            }

            var json = payload.Substring(separatorIndex + 1);

            return JsonSerializer.Deserialize(json, jsonType, serializerOptions)!;
        }

        private bool TypeIsProtocolMessage(Type type)
        {
            return type == typeof(KeepAlive) || type.Namespace == messagesNamespace;
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
