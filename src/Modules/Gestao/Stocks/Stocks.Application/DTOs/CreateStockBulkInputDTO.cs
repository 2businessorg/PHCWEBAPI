namespace Stocks.Application.DTOs;

/// <summary>
/// DTO para criação em lote de stocks.
/// </summary>
public class CreateStockBulkInputDTO
{
    public List<CreateStockInputDTO> Items { get; set; } = new();
}
