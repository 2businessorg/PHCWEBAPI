using Advances.Application.DTOs;
using MediatR;

namespace Advances.Application.Features.GetAllAdvances;

/// <summary>
/// Query para listagem paginada de Adiantamentos com filtros opcionais
/// </summary>
public sealed record GetAllAdvancesQuery(
    decimal? Ndoc,
    decimal? Rno,
    decimal? Rdano,
    decimal? No,
    int Page,
    int PageSize) : IRequest<GetAllAdvancesResultDTO>;
