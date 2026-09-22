using Currencies.Application.DTOs;
using Currencies.Domain.Repositories;
using MediatR;

namespace Currencies.Application.Features.GetCurrencyExchangeRates;

/// <summary>
/// Handler para GetCurrencyExchangeRatesQuery.
/// </summary>
public sealed class GetCurrencyExchangeRatesQueryHandler : IRequestHandler<GetCurrencyExchangeRatesQuery, IReadOnlyList<CurrencyExchangeRateOutputDTO>>
{
    private readonly ICurrencyRepository _currencyRepository;

    /// <summary>
    /// Inicializa uma nova instância do handler.
    /// </summary>
    public GetCurrencyExchangeRatesQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    /// <summary>
    /// Executa a consulta de taxas de conversão.
    /// </summary>
    public async Task<IReadOnlyList<CurrencyExchangeRateOutputDTO>> Handle(GetCurrencyExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var rates = await _currencyRepository.GetExchangeRatesByCodeAsync(request.Moeda, cancellationToken);

        return rates
            .Select(r => new CurrencyExchangeRateOutputDTO
            {
                Moeda = r.Moeda,
                Pais = r.Pais,
                CambioCompra = r.CambioCompra,
                CambioVenda = r.CambioVenda,
                Data = r.Data
            })
            .ToList();
    }
}
