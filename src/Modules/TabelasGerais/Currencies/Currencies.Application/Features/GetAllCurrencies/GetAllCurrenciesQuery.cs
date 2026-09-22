using Currencies.Application.DTOs;
using MediatR;

namespace Currencies.Application.Features.GetAllCurrencies;

/// <summary>
/// Query para obter todas as moedas disponíveis.
/// </summary>
public sealed record GetAllCurrenciesQuery : IRequest<IEnumerable<CurrencyOutputDTO>>;
