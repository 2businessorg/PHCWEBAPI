using System.Text.Json.Serialization;

namespace Stocks.Application.DTOs;

/// <summary>
/// DTO de entrada para atualização parcial de stock.
/// </summary>
public class UpdateStockInputDTO
{
    [JsonPropertyName("description")]
    public string? Descricao { get; set; }

    [JsonPropertyName("isService")]
    public bool? Eservico { get; set; }

    [JsonPropertyName("prices")]
    public List<PrecoTabelaDTO>? Precos { get; set; }

    [JsonPropertyName("taxTableId")]
    public int? TabIva { get; set; }

    [JsonPropertyName("familyRef")]
    public string? FamiliaRef { get; set; }

    [JsonPropertyName("observations")]
    public string? Obs { get; set; }

    [JsonPropertyName("isInactive")]
    public bool? Inactivo { get; set; }

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
