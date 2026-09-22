using System.Text.Json.Serialization;

namespace Currencies.Application.DTOs;

/// <summary>
/// DTO de saída para moeda.
/// </summary>
public sealed class CurrencyOutputDTO
{
    /// <summary>
    /// Código da moeda (ISO 4217).
    /// </summary>
    [JsonPropertyName("currency")]
    public string Moeda { get; set; } = string.Empty;

    /// <summary>
    /// País associado à moeda.
    /// </summary>
    [JsonPropertyName("country")]
    public string Pais { get; set; } = string.Empty;
}
