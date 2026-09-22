namespace Stocks.Application.Features.GetAllWarehouses;

using MediatR;

/// <summary>
/// Query to retrieve all warehouses
/// </summary>
public record GetAllWarehousesQuery : IRequest<GetAllWarehousesResultDTO>;
