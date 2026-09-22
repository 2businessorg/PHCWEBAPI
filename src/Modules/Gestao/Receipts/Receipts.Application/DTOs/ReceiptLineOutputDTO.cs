using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// Linha de saída de um Recibo (tabela rl)
/// </summary>
public class ReceiptLineOutputDTO
{
    [JsonPropertyName("invoiceNumber")]
    public decimal Nrdoc { get; set; }

    [JsonPropertyName("invoiceTypeId")]
    public string InvoiceTypeId { get; set; } = string.Empty;

    [JsonPropertyName("docDescription")]
    public string Cdesc { get; set; } = string.Empty;

    [JsonPropertyName("amountSettled")]
    public decimal Rec { get; set; }

    [JsonPropertyName("amountToBeSettled")]
    public decimal Eval { get; set; }

    [JsonPropertyName("documentDate")]
    public string Datalc { get; set; } = string.Empty;

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }

}
