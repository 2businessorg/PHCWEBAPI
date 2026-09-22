using MediatR;
using Stocks.Application.DTOs;

namespace Stocks.Application.Features.GetStockByRef;

public sealed record GetStockByRefQuery(string Referencia) : IRequest<StockOutputDTO?>;
