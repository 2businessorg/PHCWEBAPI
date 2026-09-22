using System.Text.Json.Serialization;
using Shared.Kernel.Responses;

namespace Receipts.Application.DTOs;

/// <summary>
/// Response DTO para criação de recibo — segue o mesmo padrão do módulo Invoices
/// (code + message + data + links).
/// </summary>
public sealed class CreateReceiptResponseDTO
{
    /// <summary>Código de resultado da operação.</summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>Mensagem de resultado ou erro.</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Dados devolvidos em caso de sucesso.</summary>
    [JsonPropertyName("data")]
    public CreateReceiptOutputDTO? Data { get; set; }

    /// <summary>Links HATEOAS associados à resposta.</summary>
    [JsonPropertyName("links")]
    public List<HATEOASLink>? Links { get; set; }

    /// <summary>Cria uma resposta de sucesso.</summary>
    /// <param name="data">Dados do recibo criado.</param>
    /// <param name="links">Links HATEOAS relacionados.</param>
    /// <returns>Envelope de sucesso no formato padrão.</returns>
    public static CreateReceiptResponseDTO Success(CreateReceiptOutputDTO data, List<HATEOASLink> links)
        => new()
        {
            Code = "0000",
            Message = "Recibo criado com sucesso",
            Data = data,
            Links = links
        };

    /// <summary>Cria uma resposta de erro.</summary>
    /// <param name="code">Código de erro.</param>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="links">Links HATEOAS de ajuda/contexto opcional.</param>
    /// <returns>Envelope de erro no formato padrão.</returns>
    public static CreateReceiptResponseDTO Error(string code, string message, List<HATEOASLink>? links = null)
        => new()
        {
            Code = code,
            Message = message,
            Data = null,
            Links = links
        };
}
