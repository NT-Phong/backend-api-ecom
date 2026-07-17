using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ecom.API.Serialization;

/// <summary>
/// Serialize DateTime as UTC ISO-8601 (with Z) and ensure Kind=Utc on read.
/// </summary>
public class DateTimeUtcConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string token for DateTime");

        var str = reader.GetString();
        if (string.IsNullOrEmpty(str))
            return default;

        // Parse using DateTimeOffset to respect provided offset/"Z" and return UTC
        if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            return dto.UtcDateTime;
        }

        throw new JsonException($"Invalid DateTime format: {str}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Normalize to UTC
        var utc = value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
        // Use 'o' (round-trip) which emits offset (Z for UTC)
        writer.WriteStringValue(utc.ToString("o", CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// Nullable DateTime converter wrapper.
/// </summary>
public class NullableDateTimeUtcConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string token for nullable DateTime");

        var str = reader.GetString();
        if (string.IsNullOrEmpty(str))
            return null;

        if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            return dto.UtcDateTime;
        }

        throw new JsonException($"Invalid DateTime format: {str}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var utc = value.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value.Value.ToUniversalTime();
        writer.WriteStringValue(utc.ToString("o", CultureInfo.InvariantCulture));
    }
}


