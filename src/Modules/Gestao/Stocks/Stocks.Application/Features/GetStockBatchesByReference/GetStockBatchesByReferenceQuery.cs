using MediatR;

namespace Stocks.Application.Features.GetStockBatchesByReference;

public record GetStockBatchesByReferenceQuery(
    string Referencia,
    string? Lote = null) : IRequest<GetStockBatchesByReferenceResultDTO>;
