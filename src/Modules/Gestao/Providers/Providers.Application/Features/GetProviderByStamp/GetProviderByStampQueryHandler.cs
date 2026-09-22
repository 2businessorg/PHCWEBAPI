using MediatR;
using Providers.Application.DTOs.Providers;
using Providers.Application.Mappings;
using Providers.Domain.Repositories;

namespace Providers.Application.Features.GetProviderByStamp;

public class GetProviderByStampQueryHandler : IRequestHandler<GetProviderByStampQuery, ProviderOutputDTO?>
{
    private readonly IProviderRepository _providerRepository;

    public GetProviderByStampQueryHandler(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<ProviderOutputDTO?> Handle(GetProviderByStampQuery request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByStampAsync(request.UProviderStamp, cancellationToken);
        
        return provider?.ToDto<ProviderOutputDTO>();
    }
}
