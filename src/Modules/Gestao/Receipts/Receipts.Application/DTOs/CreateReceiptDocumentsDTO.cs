using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// Agrupamento de documentos gerados pelo PHC WEB ao criar um recibo.
/// Pode conter um RE (recibo) e zero ou mais RDs (adiantamentos).
/// </summary>
public class CreateReceiptDocumentsDTO
{
    /// <summary>
    /// Documento de recibo principal (RE).
    /// Null se o PHC WEB não gerou um recibo (ex: pagamento 100% via adiantamento).
    /// </summary>
    [JsonPropertyName("receipt")]
    public ReceiptDocumentOutputDTO? Receipt { get; set; }

    /// <summary>
    /// Documentos de adiantamento (RD) gerados quando o valor pago excede o saldo da factura.
    /// Lista vazia na maioria dos casos.
    /// </summary>
    [JsonPropertyName("advances")]
    public List<ReceiptAdvanceOutputDTO> Advances { get; set; } = new();
}
