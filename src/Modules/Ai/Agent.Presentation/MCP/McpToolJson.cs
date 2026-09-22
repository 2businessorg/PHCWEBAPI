using System.Text.Json;

namespace Agent.Presentation.MCP;

internal static class McpToolJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal static string Serialize(object value)
        => JsonSerializer.Serialize(value, Options);

    internal static string ReadRequiredString(JsonElement arguments, string name)
    {
        if (!arguments.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
            throw new ArgumentException($"'{name}' is required.");

        var text = value.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException($"'{name}' is required.");

        return text;
    }

    internal static string? ReadOptionalString(JsonElement arguments, string name)
    {
        if (!arguments.TryGetProperty(name, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        if (value.ValueKind != JsonValueKind.String)
            throw new ArgumentException($"'{name}' must be a string.");

        var text = value.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    internal static bool ReadOptionalBool(JsonElement arguments, string name, bool defaultValue = false)
    {
        if (!arguments.TryGetProperty(name, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return defaultValue;

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(value.GetString(), out var parsed) => parsed,
            _ => throw new ArgumentException($"'{name}' must be a boolean.")
        };
    }

    internal static DateOnly ReadRequiredDate(JsonElement arguments, string name)
    {
        var text = ReadRequiredString(arguments, name);
        if (!DateOnly.TryParseExact(text, "yyyy-MM-dd", out var date))
            throw new ArgumentException($"'{name}' must use YYYY-MM-DD.");

        return date;
    }
}
