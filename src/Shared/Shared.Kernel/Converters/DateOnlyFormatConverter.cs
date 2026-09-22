using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Kernel.Converters;

/// <summary>
/// Custom JSON converter that serializes/deserializes DateTime values as "yyyy-MM-dd" date strings,
/// stripping any time component.
///
/// Usage:
/// [JsonConverter(typeof(DateOnlyFormatConverter))]
/// public DateTime Date { get; set; }
/// </summary>
public class DateOnlyFormatConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (DateTime.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;
        if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
            return fallback.Date;
        throw new JsonException($"Cannot convert \"{str}\" to DateTime. Expected format: {Format}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// Custom JSON converter that serializes/deserializes nullable DateTime? values as "yyyy-MM-dd" date strings.
/// </summary>
public class NullableDateOnlyFormatConverter : JsonConverter<DateTime?>
{
    private const string Format = "yyyy-MM-dd";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
            return null;
        if (DateTime.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;
        if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
            return fallback.Date;
        throw new JsonException($"Cannot convert \"{str}\" to DateTime?. Expected format: {Format}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
