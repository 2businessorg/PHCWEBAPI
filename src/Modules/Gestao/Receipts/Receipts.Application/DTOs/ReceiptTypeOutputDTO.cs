using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// Tipo/Série de Recibo (tabela tsre)
/// </summary>
public class ReceiptTypeOutputDTO
{
    [JsonPropertyName("docTypeId")]
    public decimal Ndoc { get; set; }

    [JsonPropertyName("docTypeName")]
    public string Nmdoc { get; set; } = string.Empty;
}
