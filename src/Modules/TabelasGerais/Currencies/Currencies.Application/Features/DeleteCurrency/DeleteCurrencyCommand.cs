using MediatR;

namespace Currencies.Application.Features.DeleteCurrency;

/// <summary>
/// Command para deletar uma moeda.
/// </summary>
public sealed record DeleteCurrencyCommand(string Moeda) : IRequest<bool>;
