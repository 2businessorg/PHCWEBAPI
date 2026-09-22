using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Kernel.Converters;

/// <summary>
/// Custom JSON converter that formats decimal values with a configurable number of decimal places.
/// By default, formats with 2 decimal places.
/// 
/// Usage:
/// [JsonConverter(typeof(DecimalFormatConverter))]
/// public decimal Value { get; set; }
/// 
/// Or with custom decimal places:
/// [JsonConverter(typeof(DecimalFormatConverter))]
/// public decimal Value { get; set; } // Adjust DecimalFormatConverter.DecimalPlaces if needed
/// </summary>
public class DecimalFormatConverter : JsonConverter<decimal>
{
    /// <summary>
    /// Number of decimal places to format to. Default: 2
    /// </summary>
    private const int DecimalPlaces = 2;

    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        // Round to specified decimal places and write as JSON number
        var rounded = Math.Round(value, DecimalPlaces);
        writer.WriteNumberValue(rounded);
    }
}

/// <summary>
/// Custom JSON converter for decimal? (nullable) values with configurable decimal places.
/// By default, formats with 2 decimal places.
/// </summary>
public class NullableDecimalFormatConverter : JsonConverter<decimal?>
{
    /// <summary>
    /// Number of decimal places to format to. Default: 2
    /// </summary>
    private const int DecimalPlaces = 2;

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
            // Round to specified decimal places and write as JSON number
            var rounded = Math.Round(value.Value, DecimalPlaces);
            writer.WriteNumberValue(rounded);
        }
    }
}
