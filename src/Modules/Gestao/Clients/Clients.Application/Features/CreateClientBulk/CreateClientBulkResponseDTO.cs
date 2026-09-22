using Clients.Application.DTOs;
using Shared.Kernel.Responses;

namespace Clients.Application.Features.CreateClientBulk;

/// <summary>
/// DTO de resposta para bulk de criação
/// </summary>
public class CreateClientBulkResponseDTO
{
    /// <summary>
    /// Código de resultado geral
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// Mensagem descritiva
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Resultados individuais de cada item (usa DTO base compartilhado)
    /// </summary>
    public List<Shared.Kernel.Responses.BulkItemResultDTO> Items { get; set; } = new();

    /// <summary>
    /// Quantidade de sucessos
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Quantidade de falhas
    /// </summary>
    public int FailureCount { get; set; }
}
