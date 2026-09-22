using System.Text.Json.Serialization;

namespace Currencies.Presentation.REST.Controllers;

/// <summary>
/// DTO de requisição para criar taxa de conversão.
/// </summary>
public sealed class CreateCurrencyExchangeRateRequest
{
    /// <summary>
    /// Código da moeda (ISO 4217).
    /// </summary>
    [JsonPropertyName("moeda")]
    public string Moeda { get; set; } = string.Empty;

    /// <summary>
    /// País associado à moeda.
    /// </summary>
    [JsonPropertyName("pais")]
    public string Pais { get; set; } = string.Empty;

    /// <summary>
    /// Quanto custa 1 unidade da moeda em moeda local (ex.: 64 MZN por 1 USD).
    /// </summary>
    [JsonPropertyName("buyRate")]
    public decimal BuyRate { get; set; }

    /// <summary>
    /// Taxa de venda.
    /// </summary>
    [JsonPropertyName("sellRate")]
    public decimal SellRate { get; set; }

    /// <summary>
    /// Data da taxa (opcional).
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime? Data { get; set; }
}
