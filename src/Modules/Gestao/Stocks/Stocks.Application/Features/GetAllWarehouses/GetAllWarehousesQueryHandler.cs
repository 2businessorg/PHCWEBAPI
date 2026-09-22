namespace Stocks.Application.Features.GetAllWarehouses;

using MediatR;
using Stocks.Domain.Repositories;
using Stocks.Application.DTOs;

/// <summary>
/// Handler for GetAllWarehousesQuery
/// </summary>
public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, GetAllWarehousesResultDTO>
{
    private readonly IStockRepository _repository;

    public GetAllWarehousesQueryHandler(IStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetAllWarehousesResultDTO> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        var domainDtos = await _repository.GetAllWarehousesAsync(cancellationToken);

        var applicationDtos = domainDtos.Select(d => new WarehouseDTO
        {
            No = d.No,
            NomeArmazem = d.NomeArmazem
        }).ToList();

        return new GetAllWarehousesResultDTO
        {
            Items = applicationDtos,
            Metadata = new Meta
            {
                TotalItems = applicationDtos.Count,
                ItemCount = applicationDtos.Count
            }
        };
    }
}
