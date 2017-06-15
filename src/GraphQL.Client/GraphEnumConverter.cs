using System;
using Newtonsoft.Json;

namespace GraphQL.Client
{
    public class GraphEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteRawValue(Enum.GetName(value.GetType(), value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var isNullable = IsNullable(objectType);
            if (reader.TokenType == JsonToken.Null)
            {
                if (!IsNullable(objectType))
                {
                    throw new JsonSerializationException($"Cannot convert null value to {objectType}.");
                }
                return null;
            }
            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    var enumText = reader.Value.ToString();
                    return Enum.Parse(objectType, enumText);
                }

                if (reader.TokenType == JsonToken.Integer)
                {
                    return Convert.ChangeType(reader.Value, Enum.GetUnderlyingType(objectType));
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType}'.", ex);
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing enum.");
        }

        public override bool CanConvert(Type objectType)
        {
            var t = IsNullable(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;

            return t.IsEnum;
        }

        private bool IsNullable(Type type)
        {
            return type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }
    }
}
