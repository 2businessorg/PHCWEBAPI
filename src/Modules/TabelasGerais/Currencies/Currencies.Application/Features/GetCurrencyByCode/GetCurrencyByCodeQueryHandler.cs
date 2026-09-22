using MediatR;
using Currencies.Application.DTOs;
using Currencies.Application.Mappings;
using Currencies.Domain.Repositories;

namespace Currencies.Application.Features.GetCurrencyByCode;

/// <summary>
/// Handler para a query GetCurrencyByCode.
/// Obtém uma moeda específica pelo código.
/// </summary>
public sealed class GetCurrencyByCodeQueryHandler : IRequestHandler<GetCurrencyByCodeQuery, CurrencyOutputDTO>
{
    private readonly ICurrencyRepository _currencyRepository;

    public GetCurrencyByCodeQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<CurrencyOutputDTO> Handle(GetCurrencyByCodeQuery request, CancellationToken cancellationToken)
    {
        var currency = await _currencyRepository.GetByCodeAsync(request.Moeda, cancellationToken);
        return currency?.ToOutputDto();
    }
}
