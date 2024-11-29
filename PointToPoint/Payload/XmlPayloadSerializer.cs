using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace PointToPoint.Payload
{
    // This class requires that payload classes have parameterless constructors
    public class XmlPayloadSerializer : AbstractTextPayloadSerializer
    {
        private readonly Dictionary<Type, XmlSerializer> serializers = new();

        public XmlPayloadSerializer(Assembly messagesAssembly) : base(messagesAssembly)
        {
        }

        protected override string SerializeObject(object message)
        {
            var serializer = GetSerializer(message.GetType());
            using var textWriter = new StringWriter();
            serializer.Serialize(textWriter, message);
            return textWriter.ToString();
        }

        protected override object? DeserializeObject(string text, Type type)
        {
            var serializer = GetSerializer(type);
            using var textReader = new StringReader(text);
            try
            {
                return serializer.Deserialize(textReader);
            }
            catch
            {
                return null;
            }
        }

        private XmlSerializer GetSerializer(Type messageType)
        {
            if (serializers.TryGetValue(messageType, out var serializer))
            {
                return serializer;
            }

            serializer = new XmlSerializer(messageType);
            serializers.Add(messageType, serializer);
            return serializer;
        }
    }
}
