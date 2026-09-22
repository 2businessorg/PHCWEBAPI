using Currencies.Application.DTOs;
using MediatR;

namespace Currencies.Application.Features.GetAllCurrencyExchangeRates;

/// <summary>
/// Query para obter todas as taxas de conversão.
/// </summary>
public sealed record GetAllCurrencyExchangeRatesQuery : IRequest<IReadOnlyList<CurrencyExchangeRateOutputDTO>>;
