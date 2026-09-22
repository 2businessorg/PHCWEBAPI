using System.Text.Json.Serialization;

namespace Receipts.Application.DTOs;

/// <summary>
/// DTO de input para criação de um Recibo via script insertReAPI do PHC WEB.
/// O código de tesouraria, local e data são resolvidos internamente pelo script.
/// </summary>
public class CreateReceiptInputDTO
{
    /// <summary>Número da configuração da série do recibo (ndoc em tsre)</summary>
    [JsonPropertyName("docTypeId")]
    public decimal Ndoc { get; set; }

    /// <summary>Número interno do cliente (no em cl)</summary>
    [JsonPropertyName("clientId")]
    public decimal No { get; set; }

    /// <summary>Número interno da conta bancária / caixa</summary>
    [JsonPropertyName("bankAccountId")]
    public decimal Contado { get; set; }

    /// <summary>Linhas de regularização de documentos</summary>
    [JsonPropertyName("lines")]
    public List<CreateReceiptLineInputDTO> Linhas { get; set; } = new();

    /// <summary>Campos adicionais específicos do tenant (opcional)</summary>
    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
