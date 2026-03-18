using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tly;

internal sealed class TlyDateTimeJsonConverter : JsonConverter<DateTimeOffset?>
{
    private const string TlyDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a string when reading a T.LY datetime value.");
        }

        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParseExact(
                value,
                TlyDateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var exact))
        {
            return exact;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            return parsed;
        }

        throw new JsonException($"Unable to parse T.LY datetime value '{value}'.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.UtcDateTime.ToString(TlyDateTimeFormat, CultureInfo.InvariantCulture));
    }
}
