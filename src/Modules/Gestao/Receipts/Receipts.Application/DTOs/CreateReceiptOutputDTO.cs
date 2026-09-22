using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// DTO de saída da operação POST /receipts.
/// Mapeia a resposta do script insertReAPI do PHC WEB para chaves em inglês.
/// </summary>
public class CreateReceiptOutputDTO
{
    /// <summary>ID do cliente para o qual o recibo foi emitido</summary>
    [JsonPropertyName("clientId")]
    public decimal ClientId { get; set; }

    /// <summary>Número interno da conta bancária / caixa usada na criação do recibo</summary>
    [JsonPropertyName("bankAccountId")]
    public decimal BankAccountId { get; set; }

    /// <summary>Total de documentos gerados (RE + RDs)</summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    /// <summary>Documentos gerados: recibo principal e eventuais adiantamentos</summary>
    [JsonPropertyName("documents")]
    public CreateReceiptDocumentsDTO Documents { get; set; } = new();
}
