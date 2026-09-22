namespace Dossiers.Application.DTOs;

/// <summary>
/// Resultado paginado de dossiers
/// </summary>
public record GetAllDossiersResultDTO(
    int TotalItems,
    int CurrentPage,
    int PageSize,
    IReadOnlyList<DossierOutputDTO> Items
);
