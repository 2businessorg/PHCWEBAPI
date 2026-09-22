using System.Text.Json.Serialization;
using Shared.Kernel.Responses;

namespace Invoices.Application.DTOs;

/// <summary>
/// Response DTO para criação de fatura com lista tipada de dados.
/// </summary>
public sealed class CreateInvoiceResponseDTO
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<InvoiceOutputDTO>? Data { get; set; }

    [JsonPropertyName("links")]
    public List<HATEOASLink>? Links { get; set; }

    public static CreateInvoiceResponseDTO Success(InvoiceOutputDTO created, List<HATEOASLink> links)
        => new()
        {
            Code = "0000",
            Message = "Fatura criada com sucesso",
            Data = new List<InvoiceOutputDTO> { created },
            Links = links
        };

    public static CreateInvoiceResponseDTO Error(string code, string message)
        => new()
        {
            Code = code,
            Message = message,
            Data = null,
            Links = null
        };
}