using MediatR;
using Stocks.Application.DTOs;
using Stocks.Domain.Repositories;

namespace Stocks.Application.Features.GetAllBatches;

public class GetAllBatchesQueryHandler : IRequestHandler<GetAllBatchesQuery, GetAllBatchesResultDTO>
{
    private readonly IStockRepository _repository;

    public GetAllBatchesQueryHandler(IStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetAllBatchesResultDTO> Handle(
        GetAllBatchesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetBatchesPagedAsync(
            request.Referencia,
            request.Lote,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Converter DTOs de Domain para Application
        var items = result.Items.Select(batch => new BatchDetailDTO
        {
            Lote = batch.Lote,
            Referencia = batch.Referencia,
            Design = batch.Design,
            Forlote = batch.Forlote,
            Stock = batch.Stock,
            Qttacout = batch.Qttacout,
            Qttacin = batch.Qttacin,
            Uintr = batch.Uintr,
            Validade = batch.Validade,
            Datafact = batch.Datafact,
            Pcult = batch.Pcult
        }).ToList();

        return new GetAllBatchesResultDTO
        {
            TotalItems = result.TotalItems,
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize,
            Items = items
        };
    }
}
