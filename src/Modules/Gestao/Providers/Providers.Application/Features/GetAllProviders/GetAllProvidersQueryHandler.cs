using MediatR;
using Providers.Application.DTOs.Providers;
using Providers.Application.Mappings;
using Providers.Domain.Repositories;

namespace Providers.Application.Features.GetAllProviders;

public class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, IEnumerable<ProviderOutputDTO>>
{
    private readonly IProviderRepository _providerRepository;

    public GetAllProvidersQueryHandler(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<IEnumerable<ProviderOutputDTO>> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
    {
        var providers = string.IsNullOrEmpty(request.Environment)
            ? await _providerRepository.GetAllAsync(request.IncludeInactive, cancellationToken)
            : await _providerRepository.GetByEnvironmentAsync(request.Environment, request.IncludeInactive, cancellationToken);

        return providers.ToDtos<ProviderOutputDTO>();
    }
}
