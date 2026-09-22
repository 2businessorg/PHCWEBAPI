using MediatR;
using Parameters.Application.DTOs;
using Parameters.Application.DTOs.Parameters;

namespace Parameters.Application.Features.GetAllParameters;

public record GetAllParametersQuery(
    bool IncludeInactive = false
) : IRequest<IEnumerable<ParameterOutputDTO>>;
