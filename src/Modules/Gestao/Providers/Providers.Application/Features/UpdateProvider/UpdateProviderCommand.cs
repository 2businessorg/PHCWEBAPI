using MediatR;
using Providers.Application.DTOs.Providers;

namespace Providers.Application.Features.UpdateProvider;

public record UpdateProviderCommand(
    string UProviderStamp,
    UpdateProviderInputDTO Dto,
    string? AtualizadoPor
) : IRequest<ProviderOutputDTO>;
