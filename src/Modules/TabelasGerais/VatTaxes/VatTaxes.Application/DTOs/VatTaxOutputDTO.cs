using System.Text.Json.Serialization;

namespace VatTaxes.Application.DTOs;

/// <summary>
/// DTO de saída para taxa de IVA
/// </summary>
public class VatTaxOutputDTO
{
    [JsonPropertyName("code")]
    public decimal Tabiva { get; set; }

    [JsonPropertyName("rate")]
    public decimal Taxa { get; set; }

    [JsonPropertyName("reference")]
    public string Ref { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Design { get; set; } = string.Empty;
}
