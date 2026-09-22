using System.Text.Json.Serialization;

namespace Shared.Kernel.Responses;

/// <summary>
/// Resposta padrão da API
/// </summary>
public sealed record ResponseDTO
{
    [JsonPropertyName("response")]
    public ResponseCodeDTO Response { get; init; }

    [JsonPropertyName("data")]
    public object? Data { get; init; }

    [JsonPropertyName("content")]
    public object? Content { get; init; }

    public ResponseDTO(ResponseCodeDTO response, object? data = null, object? content = null)
    {
        Response = response;
        Data = data;
        Content = content;
    }

    /// <summary>
    /// Cria uma resposta de sucesso com ID de correlação
    /// </summary>
    public static ResponseDTO Success(object? data = null, object? content = null, decimal? correlationId = null) 
        => new(new ResponseCodeDTO(ResponseCodes.Success.Code, ResponseCodes.Success.Description, correlationId), data, content);

    /// <summary>
    /// Cria uma resposta de erro com ID de correlação
    /// </summary>
    public static ResponseDTO Error(ResponseCodeDTO errorCode, object? data = null, object? content = null, decimal? correlationId = null)
        => new(new ResponseCodeDTO(errorCode.Code, errorCode.Description, correlationId), data, content);
}
