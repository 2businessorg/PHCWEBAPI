using MediatR;
using Parameters.Application.DTOs.Parameters;

namespace Parameters.Application.Features.GetMoedas;

/// <summary>
/// Query para obter a lista de moedas disponíveis.
/// </summary>
public record GetMoedasQuery : IRequest<IEnumerable<MoedaOutputDTO>>;
