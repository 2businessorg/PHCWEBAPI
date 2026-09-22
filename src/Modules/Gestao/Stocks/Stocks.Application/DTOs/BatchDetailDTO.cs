using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Stocks.Application.DTOs;

/// <summary>
/// DTO para representar detalhes de um lote (batch) com informações de validade e custos.
/// </summary>
public class BatchDetailDTO
{
    [JsonPropertyName("batch")]
    public string Lote { get; set; } = "";

    [JsonPropertyName("reference")]
    public string Referencia { get; set; } = "";

    [JsonPropertyName("description")]
    public string Design { get; set; } = "";

    [JsonPropertyName("supplierBatch")]
    public string Forlote { get; set; } = "";

    [JsonPropertyName("stock")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Stock { get; set; }

    [JsonPropertyName("quantityOutYear")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Qttacout { get; set; }

    [JsonPropertyName("quantityInYear")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Qttacin { get; set; }

    [JsonPropertyName("lastEntry")]
    public DateTime? Uintr { get; set; }

    [JsonPropertyName("expiryDate")]
    public DateTime? Validade { get; set; }

    [JsonPropertyName("invoiceLimitDate")]
    public DateTime? Datafact { get; set; }

    [JsonPropertyName("lastCostPrice")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Pcult { get; set; }
}
