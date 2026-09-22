using System.Text.Json.Serialization;

namespace Currencies.Application.DTOs;

/// <summary>
/// DTO de entrada para criar uma taxa de conversão.
/// </summary>
public sealed class CreateCurrencyExchangeRateInputDTO
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

    /// <summary>
    /// Taxa de câmbio de compra (deve ser maior que 0).
    /// </summary>
    [JsonPropertyName("buyRate")]
    public decimal UCambioC { get; set; }

    /// <summary>
    /// Taxa de câmbio de venda (deve ser maior que 0).
    /// </summary>
    [JsonPropertyName("sellRate")]
    public decimal UCambioV { get; set; }

    /// <summary>
    /// Data da taxa de câmbio. Se nula, usa a data atual.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime? Data { get; set; }
}
