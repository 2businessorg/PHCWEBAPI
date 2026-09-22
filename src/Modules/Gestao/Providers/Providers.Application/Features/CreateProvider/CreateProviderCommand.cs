using MediatR;
using Providers.Application.DTOs.Providers;

namespace Providers.Application.Features.CreateProvider;

public record CreateProviderCommand(
    CreateProviderInputDTO Dto,
    string? CriadoPor
) : IRequest<ProviderOutputDTO>;
