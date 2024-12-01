using System;
using System.Reflection;
using Newtonsoft.Json;

namespace PointToPoint.Payload.NewtonsoftJson
{
    public class NewtonsoftJsonPayloadSerializer : AbstractTextPayloadSerializer
    {
        private readonly JsonSerializerSettings serializerSettings;

        public NewtonsoftJsonPayloadSerializer(Assembly messagesAssembly, Formatting formatting = Formatting.None) : this(messagesAssembly, new JsonSerializerSettings() { Formatting = formatting })
        {
        }

        public NewtonsoftJsonPayloadSerializer(Assembly messagesAssembly, JsonSerializerSettings serializerSettings) : base(messagesAssembly)
        {
            this.serializerSettings = serializerSettings;
        }

        protected override string SerializeObject(object message)
        {
            return JsonConvert.SerializeObject(message, serializerSettings);
        }

        protected override object? DeserializeObject(string text, Type type)
        {
            return JsonConvert.DeserializeObject(text, type);
        }
    }
}
