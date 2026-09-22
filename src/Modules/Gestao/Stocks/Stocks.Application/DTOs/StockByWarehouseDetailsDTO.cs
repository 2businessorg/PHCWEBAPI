namespace Stocks.Application.DTOs;

using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

/// <summary>
/// Application DTO for stock data grouped by warehouse (warehouse-only view, not batch-specific)
/// with JSON serialization attributes for API response
/// </summary>
public class StockByWarehouseDetailsDTO
{
    [JsonPropertyName("warehouse")]
    public int Armazem { get; set; }

    [JsonPropertyName("warehouseName")]
    public string? NomeArmazem { get; set; }

    [JsonPropertyName("stock")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Stock { get; set; }

    [JsonPropertyName("stockCost")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal CustoStock { get; set; }

    [JsonPropertyName("location")]
    public string? Localizacao { get; set; }

    [JsonPropertyName("orderedByClients")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal EncomendadoPorClientes { get; set; }

    [JsonPropertyName("orderedFromSuppliers")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal EncomendadoAFornecedores { get; set; }

    [JsonPropertyName("minimumStock")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal StockMinimo { get; set; }

    [JsonPropertyName("quantityInReceipt")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal QuantidadeEmRecepcao { get; set; }

    [JsonPropertyName("quantityCaptive")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal QuantidadeCativada { get; set; }
}
