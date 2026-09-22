using MediatR;
using Parameters.Application.DTOs.Parameters;

namespace Parameters.Application.Features.GetParameterByStamp;

public record GetParameterByStampQuery(
    string Para1Stamp
) : IRequest<ParameterOutputDTO?>;
