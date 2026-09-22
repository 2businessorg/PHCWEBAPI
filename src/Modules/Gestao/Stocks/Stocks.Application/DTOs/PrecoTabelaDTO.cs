using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Stocks.Application.DTOs;

/// <summary>
/// DTO que representa um preço de venda para uma tabela específica.
/// </summary>
public class PrecoTabelaDTO
{
    [JsonPropertyName("table")]
    public int Tabela { get; set; }

    [JsonPropertyName("value")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Valor { get; set; }

    [JsonPropertyName("isTaxIncluded")]
    public bool IvaIncluido { get; set; }
}
