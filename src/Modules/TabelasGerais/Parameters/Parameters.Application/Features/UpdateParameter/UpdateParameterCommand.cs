using MediatR;
using Parameters.Application.DTOs;
using Parameters.Application.DTOs.Parameters;

namespace Parameters.Application.Features.UpdateParameter;

public record UpdateParameterCommand(
    string Para1Stamp,
    UpdateParameterInputDTO Dto,
    string? AtualizadoPor
) : IRequest<ParameterOutputDTO>;
