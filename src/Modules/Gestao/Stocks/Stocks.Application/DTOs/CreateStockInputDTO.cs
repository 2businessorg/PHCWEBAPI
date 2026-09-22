using System.Text.Json.Serialization;

namespace Stocks.Application.DTOs;

/// <summary>
/// DTO de entrada para criar um stock.
/// </summary>
public class CreateStockInputDTO
{
    [JsonPropertyName("reference")]
    public string Referencia { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Descricao { get; set; } = null!;

    [JsonPropertyName("isService")]
    public bool Eservico { get; set; }

    [JsonPropertyName("prices")]
    public List<PrecoTabelaDTO> Precos { get; set; } = new();

    [JsonPropertyName("taxTableId")]
    public int TabIva { get; set; }

    [JsonPropertyName("familyRef")]
    public string FamiliaRef { get; set; } = null!;

    [JsonPropertyName("observations")]
    public string Obs { get; set; } = "";

    [JsonPropertyName("isInactive")]
    public bool Inactivo { get; set; }

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
