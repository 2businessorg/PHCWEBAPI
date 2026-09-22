using System.Text.Json.Serialization;

namespace Currencies.Application.DTOs;

/// <summary>
/// DTO de entrada para criar uma moeda.
/// </summary>
public sealed class CreateCurrencyInputDTO
{
    /// <summary>
    /// Código da moeda (ISO 4217 - 2 a 3 caracteres).
    /// </summary>
    [JsonPropertyName("currency")]
    public string Moeda { get; set; } = string.Empty;

    /// <summary>
    /// País associado à moeda (máx 12 caracteres).
    /// </summary>
    [JsonPropertyName("country")]
    public string Pais { get; set; } = string.Empty;
}
