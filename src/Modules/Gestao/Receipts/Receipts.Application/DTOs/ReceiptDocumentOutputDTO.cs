using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// Documento RE (recibo) retornado pelo PHC WEB após criação.
/// </summary>
public class ReceiptDocumentOutputDTO
{
    /// <summary>Tipo/série do recibo (ndoc)</summary>
    [JsonPropertyName("docTypeId")]
    public decimal DocTypeId { get; set; }

    /// <summary>Número sequencial do recibo (rno)</summary>
    [JsonPropertyName("receiptNumber")]
    public decimal ReceiptNumber { get; set; }

    /// <summary>Ano do recibo</summary>
    [JsonPropertyName("year")]
    public int Year { get; set; }

    /// <summary>Total regularizado neste recibo</summary>
    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    /// <summary>Linhas de regularização</summary>
    [JsonPropertyName("lines")]
    public List<ReceiptDocumentLineOutputDTO> Lines { get; set; } = new();

    /// <summary>Campos adicionais específicos do tenant no documento RE</summary>
    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
