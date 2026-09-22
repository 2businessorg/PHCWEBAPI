using MediatR;
using Currencies.Application.DTOs;

namespace Currencies.Application.Features.CreateCurrency;

/// <summary>
/// Command para criar uma nova moeda.
/// </summary>
public sealed record CreateCurrencyCommand(
    CreateCurrencyInputDTO Dto,
    string? CreatedBy = null) : IRequest<CurrencyOutputDTO>;
