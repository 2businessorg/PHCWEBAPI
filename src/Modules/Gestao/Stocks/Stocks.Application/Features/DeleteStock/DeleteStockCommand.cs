using MediatR;

namespace Stocks.Application.Features.DeleteStock;

public sealed record DeleteStockCommand(string Referencia) : IRequest<bool>;
