using MediatR;
using Stocks.Application.DTOs;

namespace Stocks.Application.Features.UpdateStock;

public sealed record UpdateStockCommand(string Referencia, UpdateStockInputDTO Dto, string? UpdatedBy = null)
    : IRequest<StockOutputDTO?>;
