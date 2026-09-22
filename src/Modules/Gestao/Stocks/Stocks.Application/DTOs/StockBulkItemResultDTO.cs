using Shared.Kernel.Responses;

namespace Stocks.Application.DTOs;

/// <summary>
/// Resultado por item no processamento bulk de stocks.
/// Herda de Shared.Kernel.Responses.BulkItemResultDTO para padronização.
/// </summary>
public class StockBulkItemResultDTO : Shared.Kernel.Responses.BulkItemResultDTO
{
}

/// <summary>
/// Alias para BulkErrorDTO do Shared.Kernel (compatibilidade)
/// </summary>
public class ErrorItemDTO : BulkErrorDTO
{
}
