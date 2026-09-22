using Currencies.Application.DTOs;
using MediatR;

namespace Currencies.Application.Features.CreateCurrencyExchangeRate;

/// <summary>
/// Command para criar uma taxa de conversão.
/// </summary>
public sealed record CreateCurrencyExchangeRateCommand(
    CreateCurrencyExchangeRateInputDTO Dto,
    string? CreatedBy = null) : IRequest<CurrencyExchangeRateOutputDTO>;
