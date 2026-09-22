using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// Linha de regularização de documento numa criação de Recibo.
/// O documento é identificado pelo número, tipo e ano da factura.
/// </summary>
public class CreateReceiptLineInputDTO
{
    /// <summary>
    /// Número da factura a regularizar (fno em ft).
    /// </summary>
    [JsonPropertyName("invoiceNumber")]
    public decimal InvoiceNumber { get; set; }

    /// <summary>
    /// Tipo/série do documento (ndoc em ft). Corresponde ao docTypeId da factura.
    /// </summary>
    [JsonPropertyName("invoiceTypeId")]
    public decimal InvoiceTypeId { get; set; }

    /// <summary>
    /// Ano da factura (ftano em ft).
    /// </summary>
    [JsonPropertyName("invoiceYear")]
    public decimal InvoiceYear { get; set; }

    /// <summary>
    /// Valor a regularizar neste documento (deve ser maior que zero).
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>Campos adicionais específicos do tenant para a linha RL (opcional)</summary>
    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
