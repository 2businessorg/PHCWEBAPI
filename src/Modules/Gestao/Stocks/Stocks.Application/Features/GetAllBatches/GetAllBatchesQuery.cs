using MediatR;

namespace Stocks.Application.Features.GetAllBatches;

public record GetAllBatchesQuery(
    string? Referencia = null,
    string? Lote = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<GetAllBatchesResultDTO>;
