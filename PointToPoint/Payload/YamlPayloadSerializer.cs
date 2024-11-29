using System;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PointToPoint.Payload
{
    // This class requires that payload classes have parameterless constructors
    public class YamlPayloadSerializer : AbstractTextPayloadSerializer
    {
        private readonly ISerializer serializer;
        private readonly IDeserializer deserializer;

        public YamlPayloadSerializer(Assembly messagesAssembly) : base(messagesAssembly)
        {
            var namingConvention = CamelCaseNamingConvention.Instance;
            serializer = new SerializerBuilder().WithNamingConvention(namingConvention).Build();
            deserializer = new DeserializerBuilder().WithNamingConvention(namingConvention).Build();
        }

        protected override string SerializeObject(object message)
        {
            return serializer.Serialize(message);
        }

        protected override object? DeserializeObject(string text, Type type)
        {
            return deserializer.Deserialize(text, type);
        }
    }
}
