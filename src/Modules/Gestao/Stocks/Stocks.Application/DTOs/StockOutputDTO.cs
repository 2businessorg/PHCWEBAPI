using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Stocks.Application.DTOs;

/// <summary>
/// DTO de saída de stock.
/// </summary>
public class StockOutputDTO
{
    [JsonPropertyName("reference")]
    public string Referencia { get; set; } = "";

    [JsonPropertyName("description")]
    public string Descricao { get; set; } = "";

    [JsonPropertyName("isService")]
    public bool Eservico { get; set; }

    [JsonPropertyName("prices")]
    public List<PrecoTabelaDTO> Precos { get; set; } = new();

    [JsonPropertyName("quantity")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Stock { get; set; }

    [JsonPropertyName("taxTableId")]
    public int TabIva { get; set; }

    [JsonPropertyName("familyRef")]
    public string FamiliaRef { get; set; } = "";

    [JsonPropertyName("familyName")]
    public string FamiliaNome { get; set; } = "";

    [JsonPropertyName("observations")]
    public string Obs { get; set; } = "";

    [JsonPropertyName("isInactive")]
    public bool Inactivo { get; set; }

    [JsonPropertyName("useBatches")]
    public bool UsaLote { get; set; }

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }

}
