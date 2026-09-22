using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Stocks.Application.DTOs;

/// <summary>
/// DTO para representar stock de um lote por armazém.
/// </summary>
public class StockByWarehouseDTO
{
    [JsonPropertyName("batch")]
    public string Lote { get; set; } = "";

    [JsonPropertyName("reference")]
    public string Referencia { get; set; } = "";

    [JsonPropertyName("warehouse")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Armazem { get; set; }

    [JsonPropertyName("warehouseName")]
    public string NomeArmazem { get; set; } = "";

    [JsonPropertyName("stock")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Stock { get; set; }

    [JsonPropertyName("location")]
    public string Localizacao { get; set; } = "";
}
