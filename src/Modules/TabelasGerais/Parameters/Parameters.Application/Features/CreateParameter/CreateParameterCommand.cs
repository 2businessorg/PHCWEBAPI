using MediatR;
using Parameters.Application.DTOs.Parameters;

namespace Parameters.Application.Features.CreateParameter;

public record CreateParameterCommand(
    CreateParameterInputDTO Dto,
    string? CriadoPor
) : IRequest<ParameterOutputDTO>;
