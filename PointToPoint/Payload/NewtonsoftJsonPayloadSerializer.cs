using System;
using System.Text;
using Newtonsoft.Json;
using PointToPoint.Protocol;

namespace PointToPoint.Payload
{
    public class NewtonsoftJsonPayloadSerializer : IPayloadSerializer
    {
        private static readonly Encoding PayloadTextEncoding = Encoding.Unicode;
        private const string IdPayloadSeparator = " ";

        private readonly JsonSerializerSettings serializerSettings;
        private readonly string messagesNamespace;

        public NewtonsoftJsonPayloadSerializer(string messagesNamespace, Formatting formatting = Formatting.None) : this(messagesNamespace, new JsonSerializerSettings() { Formatting = formatting })
        {
        }

        public NewtonsoftJsonPayloadSerializer(string messagesNamespace, JsonSerializerSettings serializerSettings)
        {
            this.messagesNamespace = messagesNamespace;
            this.serializerSettings = serializerSettings;
        }

        public byte[] MessageToPayload(object message)
        {
            var messageType = message.GetType();
            if (!TypeIsProtocolMessage(messageType))
            {
                throw new ArgumentException($"Non protocol message can not be sent: {messageType}");
            }
            var json = JsonConvert.SerializeObject(message, serializerSettings);
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

            var jsonTypeString = payload.Substring(0, separatorIndex);
            var jsonType = Type.GetType(jsonTypeString);
            if (jsonType == null)
            {
                throw new Exception($"Unknown message type: {jsonTypeString}");
            }

            if (!TypeIsProtocolMessage(jsonType))
            {
                throw new Exception($"Non protocol message type received: {jsonTypeString}");
            }

            var json = payload.Substring(separatorIndex + 1);

            var message = JsonConvert.DeserializeObject(json, jsonType);
            if (message is null)
            {
                throw new Exception("Deserialize returned null");
            }
            return message;
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
