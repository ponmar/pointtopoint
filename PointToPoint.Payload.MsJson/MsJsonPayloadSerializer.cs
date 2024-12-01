using System;
using System.Text.Json;
using System.Reflection;

namespace PointToPoint.Payload.MsJson
{
    public class MsJsonPayloadSerializer : AbstractTextPayloadSerializer
    {
        private readonly JsonSerializerOptions? serializerOptions;

        public MsJsonPayloadSerializer(Assembly messagesAssembly, JsonSerializerOptions? serializerOptions = null) : base(messagesAssembly)
        {
            this.serializerOptions = serializerOptions;
        }

        protected override string SerializeObject(object message)
        {
            return JsonSerializer.Serialize(message);
        }

        protected override object? DeserializeObject(string text, Type type)
        {
            return JsonSerializer.Deserialize(text, type, serializerOptions);
        }
    }
}
