using Stocks.Application.DTOs;

namespace Stocks.Application.Features.GetAllBatches;

public class GetAllBatchesResultDTO
{
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public List<BatchDetailDTO> Items { get; set; } = new();
}
