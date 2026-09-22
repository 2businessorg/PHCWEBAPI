using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// Documento RD (adiantamento) retornado pelo PHC WEB quando há excesso de pagamento.
/// </summary>
public class ReceiptAdvanceOutputDTO
{
    /// <summary>Tipo/série do adiantamento (ndoc)</summary>
    [JsonPropertyName("docTypeId")]
    public decimal DocTypeId { get; set; }

    /// <summary>Número sequencial do adiantamento (rno)</summary>
    [JsonPropertyName("receiptNumber")]
    public decimal ReceiptNumber { get; set; }

    /// <summary>Total do adiantamento</summary>
    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    /// <summary>Número da factura de origem (nrdoc)</summary>
    [JsonPropertyName("invoiceNumber")]
    public decimal InvoiceNumber { get; set; }

    /// <summary>Ano da factura de origem (ftano)</summary>
    [JsonPropertyName("invoiceYear")]
    public decimal InvoiceYear { get; set; }
}
