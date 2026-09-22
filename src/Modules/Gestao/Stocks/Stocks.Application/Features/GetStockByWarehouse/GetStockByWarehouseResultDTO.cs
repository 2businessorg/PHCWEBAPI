namespace Stocks.Application.Features.GetStockByWarehouse;

using Stocks.Application.DTOs;
using Shared.Kernel.Responses;

/// <summary>
/// Result DTO for GetStockByWarehouseQuery containing warehouse stock data
/// </summary>
public class GetStockByWarehouseResultDTO
{
    public List<StockByWarehouseDetailsDTO> Items { get; set; } = [];
    public Meta Metadata { get; set; } = new();
    public List<HATEOASLink> Links { get; set; } = [];
}

public class Meta
{
    public int TotalItems { get; set; }
    public int ItemCount { get; set; }
}
