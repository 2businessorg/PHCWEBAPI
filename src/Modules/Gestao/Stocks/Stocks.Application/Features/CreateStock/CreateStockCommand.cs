using MediatR;
using Stocks.Application.DTOs;

namespace Stocks.Application.Features.CreateStock;

public sealed record CreateStockCommand(CreateStockInputDTO Dto, string? CreatedBy = null)
    : IRequest<StockOutputDTO>;
