using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// Representação de saída de um Recibo (cabeçalho Re)
/// </summary>
public class ReceiptOutputDTO
{
    [JsonPropertyName("docTypeId")]
    public decimal Ndoc { get; set; }

    [JsonPropertyName("docTypeName")]
    public string Nmdoc { get; set; } = string.Empty;

    [JsonPropertyName("receiptNumber")]
    public decimal Rno { get; set; }

    [JsonPropertyName("year")]
    public decimal Reano { get; set; }

    [JsonPropertyName("date")]
    public string Rdata { get; set; } = string.Empty;

    [JsonPropertyName("clientId")]
    public decimal No { get; set; }

    [JsonPropertyName("clientName")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("totalForeignCurrency")]
    public decimal Totalmoeda { get; set; }

    [JsonPropertyName("currency")]
    public string Moeda { get; set; } = string.Empty;

    [JsonPropertyName("bankAccountId")]
    public decimal Contado { get; set; }

    [JsonPropertyName("bankAccountName")]
    public string BankAccountName { get; set; } = string.Empty;

    [JsonPropertyName("lines")]
    public List<ReceiptLineOutputDTO>? Lines { get; set; }

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
