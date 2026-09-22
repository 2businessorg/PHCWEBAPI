using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Kernel.Converters;

/// <summary>
/// Custom JSON converter that ensures integer values are serialized without decimal places.
/// Useful for fields that are technically decimal but represent whole numbers (0 decimal places).
/// 
/// Usage:
/// [JsonConverter(typeof(IntegerFormatConverter))]
/// public int TaxTableId { get; set; }
/// </summary>
public class IntegerFormatConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// Custom JSON converter for nullable integer values.
/// Ensures nullable integers are serialized without decimal places.
/// Returns null if the value is null.
/// </summary>
public class NullableIntegerFormatConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}

/// <summary>
/// Custom JSON converter that formats decimal values representing integers with 0 decimal places.
/// This is useful for decimal fields that should display as whole numbers (e.g., TabIva field).
/// 
/// Usage:
/// [JsonConverter(typeof(DecimalAsIntegerFormatConverter))]
/// public decimal TaxTableId { get; set; }
/// 
/// Behavior:
/// - Input: 3.0 or 3.00 → Output: "3"
/// - Input: 5.5 (will be truncated) → Output: "5"
/// </summary>
public class DecimalAsIntegerFormatConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        // Format as integer (0 decimal places) using banker's rounding
        var intValue = (int)Math.Round(value, 0);
        writer.WriteNumberValue(intValue);
    }
}

/// <summary>
/// Custom JSON converter that formats nullable decimal values representing integers with 0 decimal places.
/// </summary>
public class NullableDecimalAsIntegerFormatConverter : JsonConverter<decimal?>
{
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Null ? null : reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            // Format as integer (0 decimal places) using banker's rounding
            var intValue = (int)Math.Round(value.Value, 0);
            writer.WriteNumberValue(intValue);
        }
    }
}
