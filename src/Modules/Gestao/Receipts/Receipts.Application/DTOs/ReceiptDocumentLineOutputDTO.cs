using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// Linha de regularização no documento RE da resposta de criação de recibo.
/// Mapeia os campos da resposta do PHC WEB (linhas[]).
/// </summary>
public class ReceiptDocumentLineOutputDTO
{
    /// <summary>Número da factura regularizada (nrdoc)</summary>
    [JsonPropertyName("invoiceNumber")]
    public decimal InvoiceNumber { get; set; }

    /// <summary>Ano da factura (ftano)</summary>
    [JsonPropertyName("invoiceYear")]
    public decimal InvoiceYear { get; set; }

    /// <summary>Descrição do documento regularizado (cdesc)</summary>
    [JsonPropertyName("docDescription")]
    public string DocDescription { get; set; } = string.Empty;

    /// <summary>Valor efectivamente regularizado (erec)</summary>
    [JsonPropertyName("amountSettled")]
    public decimal AmountSettled { get; set; }

    /// <summary>Valor total do documento (eval)</summary>
    [JsonPropertyName("amountTotal")]
    public decimal AmountTotal { get; set; }

    /// <summary>Campos adicionais específicos do tenant na linha do recibo</summary>
    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
