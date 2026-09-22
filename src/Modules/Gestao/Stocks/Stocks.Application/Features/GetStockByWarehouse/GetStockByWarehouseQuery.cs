namespace Stocks.Application.Features.GetStockByWarehouse;

using MediatR;

/// <summary>
/// Query to retrieve stock data grouped by warehouse (warehouse-only view, not batch-specific)
/// </summary>
public record GetStockByWarehouseQuery(string Referencia) : IRequest<GetStockByWarehouseResultDTO>;
