using Shared.Kernel.Responses;

namespace Dossiers.Application.Features.CreateDossiersBulk;

/// <summary>
/// DTO de resposta para bulk de criação
/// </summary>
public class CreateDossiersBulkResponseDTO
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
    public List<BulkItemResultDTO> Items { get; set; } = new();

    /// <summary>
    /// Quantidade de sucessos
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Quantidade de falhas
    /// </summary>
    public int FailureCount { get; set; }
}
