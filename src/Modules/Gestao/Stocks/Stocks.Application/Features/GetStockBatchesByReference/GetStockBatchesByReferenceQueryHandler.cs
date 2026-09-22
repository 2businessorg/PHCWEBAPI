using MediatR;
using Stocks.Application.DTOs;
using Stocks.Domain.Repositories;

namespace Stocks.Application.Features.GetStockBatchesByReference;

public class GetStockBatchesByReferenceQueryHandler : IRequestHandler<GetStockBatchesByReferenceQuery, GetStockBatchesByReferenceResultDTO>
{
    private readonly IStockRepository _repository;

    public GetStockBatchesByReferenceQueryHandler(IStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetStockBatchesByReferenceResultDTO> Handle(
        GetStockBatchesByReferenceQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetBatchesByReferenceAsync(
            request.Referencia,
            request.Lote,
            cancellationToken);

        // Converter DTOs de Domain para Application
        var warehouseItems = items.Select(batch => new StockByWarehouseDTO
        {
            Lote = batch.Lote,
            Referencia = batch.Referencia,
            Armazem = batch.Armazem,
            NomeArmazem = batch.NomeArmazem,
            Stock = batch.Stock,
            Localizacao = batch.Localizacao
        }).ToList();

        return new GetStockBatchesByReferenceResultDTO
        {
            Items = warehouseItems,
            Metadata = new GetStockBatchesByReferenceResultDTO.Meta
            {
                TotalItems = warehouseItems.Count,
                ItemCount = warehouseItems.Count
            }
        };
    }
}
