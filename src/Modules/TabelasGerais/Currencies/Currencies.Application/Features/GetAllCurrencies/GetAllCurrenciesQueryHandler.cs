using Currencies.Application.DTOs;
using Currencies.Application.Mappings;
using Currencies.Domain.Repositories;
using MediatR;

namespace Currencies.Application.Features.GetAllCurrencies;

/// <summary>
/// Handler para listar todas as moedas disponíveis.
/// </summary>
public sealed class GetAllCurrenciesQueryHandler : IRequestHandler<GetAllCurrenciesQuery, IEnumerable<CurrencyOutputDTO>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public GetAllCurrenciesQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<IEnumerable<CurrencyOutputDTO>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var currencies = await _currencyRepository.GetAllAsync(cancellationToken);
        return currencies.ToOutputDtos();
    }
}
