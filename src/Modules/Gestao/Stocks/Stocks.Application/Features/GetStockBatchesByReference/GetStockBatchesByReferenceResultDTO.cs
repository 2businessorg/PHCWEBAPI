using Shared.Kernel.Responses;
using Stocks.Application.DTOs;

namespace Stocks.Application.Features.GetStockBatchesByReference;

public class GetStockBatchesByReferenceResultDTO
{
    public List<StockByWarehouseDTO> Items { get; set; } = new();

    public class Meta
    {
        public int TotalItems { get; set; }
        public int ItemCount { get; set; }
    }

    public Meta Metadata { get; set; } = new();

    public List<HATEOASLink> Links { get; set; } = new();
}
