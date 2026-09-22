using Dossiers.Application.DTOs;
using MediatR;

namespace Dossiers.Application.Features.GetAllDossiers;

/// <summary>
/// Query para listar dossiers com paginação
/// </summary>
public record GetAllDossiersQuery(
    decimal? Ndos,
    string? Nmdos,
    decimal? Obrano,
    decimal? Boano,
    decimal? No,
    decimal? Estab,
    string? Nome,
    int Page = 1,
    int PageSize = 20,
    bool IncludeLinhas = false
) : IRequest<GetAllDossiersResultDTO>;
