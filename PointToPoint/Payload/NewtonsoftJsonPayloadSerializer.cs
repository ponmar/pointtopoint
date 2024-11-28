﻿using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using PointToPoint.Protocol;

namespace PointToPoint.Payload
{
    public class NewtonsoftJsonPayloadSerializer : IPayloadSerializer
    {
        private static readonly Encoding PayloadTextEncoding = Encoding.Unicode;
        private const string IdPayloadSeparator = " ";

        private readonly Assembly[] messagesAssemblies;

        private readonly JsonSerializerSettings serializerSettings;

        public NewtonsoftJsonPayloadSerializer(Assembly messagesAssembly, Formatting formatting = Formatting.None) : this(messagesAssembly, new JsonSerializerSettings() { Formatting = formatting })
        {
        }

        public NewtonsoftJsonPayloadSerializer(Assembly messagesAssembly, JsonSerializerSettings serializerSettings)
        {
            this.messagesAssemblies = new[] { messagesAssembly, typeof(KeepAlive).Assembly };
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
            var messageString = $"{messageType}{IdPayloadSeparator}{json}";
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
            var jsonType = GetMessageType(jsonTypeString);
            if (jsonType == null)
            {
                throw new PayloadDeserializeException($"Unknown message type: {jsonTypeString}");
            }

            var json = payload.Substring(separatorIndex + 1);

            var message = JsonConvert.DeserializeObject(json, jsonType);
            if (message is null)
            {
                throw new PayloadDeserializeException($"JsonConvert.DeserializeObject() returned null for type {jsonTypeString}");
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
