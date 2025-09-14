// Converters/StringOrNumberConverter.cs
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClinicaApp.Converters
{
    public class StringOrNumberConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString() ?? string.Empty;

                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long longValue))
                    {
                        return longValue.ToString();
                    }
                    else if (reader.TryGetDouble(out double doubleValue))
                    {
                        return doubleValue.ToString();
                    }
                    break;

                case JsonTokenType.Null:
                    return string.Empty;
            }

            throw new JsonException($"Cannot convert {reader.TokenType} to string");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}