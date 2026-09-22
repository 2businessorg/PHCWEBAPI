namespace Stocks.Application.Features.GetAllWarehouses;

using Stocks.Application.DTOs;
using Shared.Kernel.Responses;

/// <summary>
/// Result DTO for GetAllWarehousesQuery
/// </summary>
public class GetAllWarehousesResultDTO
{
    public List<WarehouseDTO> Items { get; set; } = [];
    public Meta Metadata { get; set; } = new();
    public List<HATEOASLink> Links { get; set; } = [];
}

public class Meta
{
    public int TotalItems { get; set; }
    public int ItemCount { get; set; }
}
