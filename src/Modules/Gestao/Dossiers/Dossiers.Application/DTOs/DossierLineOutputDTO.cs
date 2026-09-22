using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Dossiers.Application.DTOs;

/// <summary>
/// DTO de saída para uma linha do dossier
/// </summary>
public class DossierLineOutputDTO
{
    [JsonPropertyName("productCode")]
    public string Ref { get; set; } = string.Empty;

    [JsonPropertyName("productName")]
    public string Design { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Qtt { get; set; }

    [JsonPropertyName("vatCode")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal tabIva { get; set; }

    [JsonPropertyName("vatIncluded")]
    public bool IvaIncl { get; set; }

    [JsonPropertyName("unitPrice")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal PrecoUnitario { get; set; }

    [JsonPropertyName("total")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Total { get; set; }

    [JsonPropertyName("vatRate")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal iva { get; set; }

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
