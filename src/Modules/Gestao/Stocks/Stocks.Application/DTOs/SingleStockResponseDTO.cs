using global::Shared.Kernel.DTOs;

namespace Stocks.Application.DTOs;

/// <summary>
/// Response DTO for a single stock item with HATEOAS links
/// Uses the standard SingleItemResponseDTO&lt;T&gt; pattern
/// </summary>
public class SingleStockResponseDTO : global::Shared.Kernel.DTOs.SingleItemResponseDTO<StockOutputDTO>
{
}
