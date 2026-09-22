using Currencies.Application.DTOs;
using MediatR;

namespace Currencies.Application.Features.GetCurrencyExchangeRates;

/// <summary>
/// Query para obter taxas de conversão por moeda.
/// </summary>
public sealed record GetCurrencyExchangeRatesQuery(string Moeda) : IRequest<IReadOnlyList<CurrencyExchangeRateOutputDTO>>;
