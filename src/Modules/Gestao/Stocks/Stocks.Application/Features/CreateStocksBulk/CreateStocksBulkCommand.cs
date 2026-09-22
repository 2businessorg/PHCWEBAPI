using MediatR;
using Shared.Kernel.Responses;
using Stocks.Application.DTOs;

namespace Stocks.Application.Features.CreateStocksBulk;

public sealed record CreateStocksBulkCommand(CreateStockBulkInputDTO Dto, string? CreatedBy = null)
    : IRequest<CreateStocksBulkResponseDTO>;

public sealed class CreateStocksBulkResponseDTO
{
    public string Code { get; set; } = "0000";
    public string Message { get; set; } = "";
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<Shared.Kernel.Responses.BulkItemResultDTO> Items { get; set; } = new();
}
