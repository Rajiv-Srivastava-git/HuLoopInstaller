using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Installer.Models
{
    /// <summary>
    /// Custom JSON converter for Dictionary<string, object> to handle nested JSON structures
    /// </summary>
    public class DictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
            }

            var dictionary = new Dictionary<string, object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                reader.Read();

                dictionary.Add(propertyName, ExtractValue(ref reader, options));
            }

            return dictionary;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<string, object> kvp in value)
            {
                writer.WritePropertyName(kvp.Key);
                WriteValue(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }

        private object ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString() ?? string.Empty;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long l))
                    {
                        return l;
                    }
                    return reader.GetDouble();
                case JsonTokenType.StartObject:
                    return Read(ref reader, typeof(Dictionary<string, object>), options);
                case JsonTokenType.StartArray:
                    var list = new List<object>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        list.Add(ExtractValue(ref reader, options));
                    }
                    return list;
                case JsonTokenType.Null:
                    return null!;
                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");
            }
        }

        private void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else if (value is string stringValue)
            {
                writer.WriteStringValue(stringValue);
            }
            else if (value is long longValue)
            {
                writer.WriteNumberValue(longValue);
            }
            else if (value is int intValue)
            {
                writer.WriteNumberValue(intValue);
            }
            else if (value is double doubleValue)
            {
                writer.WriteNumberValue(doubleValue);
            }
            else if (value is bool boolValue)
            {
                writer.WriteBooleanValue(boolValue);
            }
            else if (value is Dictionary<string, object> dict)
            {
                Write(writer, dict, options);
            }
            else if (value is List<object> list)
            {
                writer.WriteStartArray();
                foreach (var item in list)
                {
                    WriteValue(writer, item, options);
                }
                writer.WriteEndArray();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }
    }
}
