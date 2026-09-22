using System.Text.Json.Serialization;

namespace Shared.Kernel.Responses;

/// <summary>
/// Resposta paginada com suporte a HATEOAS
/// Estrutura limpa e intuitiva para APIs RESTful
/// </summary>
public sealed record PaginatedResponseDTO
{
    [JsonPropertyName("code")]
    public string Code { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; }

    [JsonPropertyName("data")]
    public IEnumerable<object>? Data { get; init; }

    [JsonPropertyName("links")]
    public IEnumerable<HATEOASLink>? Links { get; init; }

    public PaginatedResponseDTO(
        string code,
        string message,
        IEnumerable<object>? data = null,
        IEnumerable<HATEOASLink>? links = null)
    {
        Code = code;
        Message = message;
        Data = data;
        Links = links;
    }

    /// <summary>
    /// Cria uma resposta de sucesso
    /// </summary>
    public static PaginatedResponseDTO SuccessList(
        IEnumerable<object>? data = null,
        IEnumerable<HATEOASLink>? links = null)
        => new("0000", "Success", data, links);

    /// <summary>
    /// Cria uma resposta de sucesso com um único item
    /// </summary>
    public static PaginatedResponseDTO SuccessSingle(
        object? data = null,
        IEnumerable<HATEOASLink>? links = null)
        => new("0000", "Success", data != null ? new[] { data } : null, links);

    /// <summary>
    /// Cria uma resposta de erro
    /// </summary>
    public static PaginatedResponseDTO Error(
        string code,
        string message)
        => new(code, message, null, null);
}
