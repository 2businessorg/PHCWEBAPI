using MediatR;
using Providers.Domain.Repositories;

namespace Providers.Application.Features.DeleteProvider;

public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, bool>
{
    private readonly IProviderRepository _providerRepository;

    public DeleteProviderCommandHandler(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<bool> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByStampAsync(request.UProviderStamp, cancellationToken);

        if (provider == null)
            return false;

        await _providerRepository.DeleteAsync(provider, cancellationToken);

        return true;
    }
}
