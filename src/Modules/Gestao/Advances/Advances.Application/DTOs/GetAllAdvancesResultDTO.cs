namespace Advances.Application.DTOs;

/// <summary>
/// Resultado paginado para listagem de Adiantamentos
/// </summary>
public sealed record GetAllAdvancesResultDTO(
    int TotalItems,
    int CurrentPage,
    int PageSize,
    IReadOnlyList<AdvanceOutputDTO> Items);
