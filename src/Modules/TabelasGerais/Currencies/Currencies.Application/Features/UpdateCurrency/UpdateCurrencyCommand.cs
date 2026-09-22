using MediatR;
using Currencies.Application.DTOs;

namespace Currencies.Application.Features.UpdateCurrency;

/// <summary>
/// Command para atualizar uma moeda existente.
/// </summary>
public sealed record UpdateCurrencyCommand(string Moeda, string Pais, string? UpdatedBy = null) : IRequest<CurrencyOutputDTO?>;
