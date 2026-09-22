using System.Text.Json.Serialization;

namespace Invoices.Application.DTOs;

/// <summary>
/// DTO de saída para tipo de série de facturação
/// </summary>
public class InvoiceTypeOutputDTO
{
    /// <summary>Número/código do tipo de documento</summary>
    [JsonPropertyName("docTypeId")]
    public int Ndoc { get; set; }

    /// <summary>Nome do tipo de documento</summary>
    [JsonPropertyName("docTypeName")]
    public string Nmdoc { get; set; } = string.Empty;
}
