using System.Text.Json.Serialization;

namespace VatTaxes.Application.DTOs;

/// <summary>
/// DTO de entrada para atualização da taxa de IVA
/// </summary>
public class UpdateVatTaxInputDTO
{
    [JsonPropertyName("rate")]
    public decimal Taxa { get; set; }

    [JsonPropertyName("reference")]
    public string? Ref { get; set; }

    [JsonPropertyName("description")]
    public string? Design { get; set; }
}
