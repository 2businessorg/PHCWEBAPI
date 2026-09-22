using Advances.Application.DTOs;
using MediatR;

namespace Advances.Application.Features.GetAdvanceTypes;

/// <summary>
/// Query para listar os tipos/séries de adiantamento disponíveis (tabela tsrd)
/// </summary>
public sealed record GetAdvanceTypesQuery() : IRequest<IReadOnlyList<AdvanceTypeOutputDTO>>;
