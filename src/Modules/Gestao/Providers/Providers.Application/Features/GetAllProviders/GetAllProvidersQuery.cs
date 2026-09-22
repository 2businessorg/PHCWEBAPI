using MediatR;
using Providers.Application.DTOs.Providers;

namespace Providers.Application.Features.GetAllProviders;

public record GetAllProvidersQuery(
    bool IncludeInactive = false,
    string? Environment = null
) : IRequest<IEnumerable<ProviderOutputDTO>>;
