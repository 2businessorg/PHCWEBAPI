using MediatR;
using Currencies.Application.DTOs;

namespace Currencies.Application.Features.GetCurrencyByCode;

/// <summary>
/// Query para obter uma moeda específica pelo código.
/// </summary>
public sealed record GetCurrencyByCodeQuery(string Moeda) : IRequest<CurrencyOutputDTO>;
