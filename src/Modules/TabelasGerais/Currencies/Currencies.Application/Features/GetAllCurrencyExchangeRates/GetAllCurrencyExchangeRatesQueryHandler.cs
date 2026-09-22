using Currencies.Application.DTOs;
using Currencies.Domain.Repositories;
using MediatR;

namespace Currencies.Application.Features.GetAllCurrencyExchangeRates;

/// <summary>
/// Handler para GetAllCurrencyExchangeRatesQuery.
/// </summary>
public sealed class GetAllCurrencyExchangeRatesQueryHandler : IRequestHandler<GetAllCurrencyExchangeRatesQuery, IReadOnlyList<CurrencyExchangeRateOutputDTO>>
{
    private readonly ICurrencyRepository _currencyRepository;

    /// <summary>
    /// Inicializa uma nova instância do handler.
    /// </summary>
    public GetAllCurrencyExchangeRatesQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    /// <summary>
    /// Executa a consulta de todas as taxas de conversão.
    /// </summary>
    public async Task<IReadOnlyList<CurrencyExchangeRateOutputDTO>> Handle(GetAllCurrencyExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var rates = await _currencyRepository.GetAllExchangeRatesAsync(cancellationToken);

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
