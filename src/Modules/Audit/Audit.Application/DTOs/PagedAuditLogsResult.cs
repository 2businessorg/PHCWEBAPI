namespace Audit.Application.DTOs;

/// <summary>
/// Resultado paginado para listagem de logs de auditoria
/// </summary>
public sealed class PagedAuditLogsResult
{
    /// <summary>
    /// Lista de logs da página atual
    /// </summary>
    public IEnumerable<AuditLogOutputDTO> Items { get; init; } = new List<AuditLogOutputDTO>();

    /// <summary>
    /// Número da página atual (1-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Tamanho da página
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total de registros encontrados (sem paginação)
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total de páginas disponíveis
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Indica se existe próxima página
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Indica se existe página anterior
    /// </summary>
    public bool HasPreviousPage { get; init; }
}
