using System;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PointToPointProtocol;
using System.Linq;

namespace PointToPoint.Payload
{
    public class YamlPayloadSerializer : IPayloadSerializer
    {
        private static readonly Encoding PayloadTextEncoding = Encoding.Unicode;
        private const string IdPayloadSeparator = " ";

        private readonly Assembly[] messagesAssemblies;

        private readonly ISerializer serializer;
        private readonly IDeserializer deserializer;

        public YamlPayloadSerializer(Assembly messagesAssembly)
        {
            messagesAssemblies = new[] { messagesAssembly, typeof(KeepAlive).Assembly };

            var namingConvention = CamelCaseNamingConvention.Instance;
            serializer = new SerializerBuilder().WithNamingConvention(namingConvention).Build();
            deserializer = new DeserializerBuilder().WithNamingConvention(namingConvention).Build();
        }

        public byte[] MessageToPayload(object message)
        {
            var messageType = message.GetType();
            if (!TypeIsProtocolMessage(messageType))
            {
                throw new ArgumentException($"Non protocol message can not be sent: {messageType}");
            }

            var yaml = serializer.Serialize(message);
            var messageString = $"{messageType}{IdPayloadSeparator}{yaml}";
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

            var typeString = payload.Substring(0, separatorIndex);
            var type = GetMessageType(typeString);
            if (type == null)
            {
                throw new PayloadDeserializeException($"Unknown message type: {typeString}");
            }

            var yaml = payload.Substring(separatorIndex + 1);

            var message = deserializer.Deserialize(yaml, type);
            if (message is null)
            {
                throw new PayloadDeserializeException($"Deserialize() returned null for type {typeString}");
            }
            return message;
        }

        private Type? GetMessageType(string typeString)
        {
            foreach (var assembly in messagesAssemblies)
            {
                var type = assembly.GetType(typeString);
                if (type is not null)
                {
                    return type;
                }
            }
            return null;
        }

        private bool TypeIsProtocolMessage(Type type)
        {
            return messagesAssemblies.Contains(type.Assembly);
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
