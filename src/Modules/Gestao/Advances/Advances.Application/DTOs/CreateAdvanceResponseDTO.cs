using Shared.Kernel.Responses;
using System.Text.Json.Serialization;

namespace Advances.Application.DTOs;

/// <summary>
/// Envelope de resposta para criação de Adiantamento (com HATEOAS)
/// </summary>
public sealed class CreateAdvanceResponseDTO
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public CreateAdvanceOutputDTO? Data { get; set; }

    [JsonPropertyName("links")]
    public List<HATEOASLink>? Links { get; set; }

    public static CreateAdvanceResponseDTO Success(CreateAdvanceOutputDTO data, List<HATEOASLink> links)
        => new() { Code = "0000", Message = "Adiantamento criado com sucesso", Data = data, Links = links };

    public static CreateAdvanceResponseDTO Error(string code, string message, List<HATEOASLink>? links = null)
        => new() { Code = code, Message = message, Links = links };
}
