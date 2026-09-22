using Shared.Kernel.Responses;

namespace Clients.Application.DTOs;

/// <summary>
/// DTO de resposta para operação de bulk
/// </summary>
public class BulkResponseDTO
{
    /// <summary>
    /// Código de resultado
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// Mensagem descritiva
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Resultados individuais de cada item
    /// </summary>
    public List<Shared.Kernel.Responses.BulkItemResultDTO> Data { get; set; } = new();

    /// <summary>
    /// Quantidade de sucessos
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Quantidade de falhas
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Links HATEOAS
    /// </summary>
    public List<HATEOASLink>? Links { get; set; }
}
