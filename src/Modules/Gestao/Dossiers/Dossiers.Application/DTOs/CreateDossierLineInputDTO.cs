using System.Text.Json.Serialization;

namespace Dossiers.Application.DTOs;

/// <summary>
/// DTO de entrada para linha de dossier
/// </summary>
public class CreateDossierLineInputDTO
{
    [JsonPropertyName("productCode")]
    public string Referencia { get; set; } = string.Empty;

    [JsonPropertyName("productName")]
    public string Descricao { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantidade { get; set; }

    [JsonPropertyName("vatCode")]
    public decimal? TabIva { get; set; }

    [JsonPropertyName("vatIncluded")]
    public bool? IvaIncl { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal? PrecoUnitario { get; set; }

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
