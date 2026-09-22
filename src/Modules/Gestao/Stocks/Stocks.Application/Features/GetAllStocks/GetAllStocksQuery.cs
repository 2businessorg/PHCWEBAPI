using MediatR;
using Stocks.Application.DTOs;

namespace Stocks.Application.Features.GetAllStocks;

public sealed record GetAllStocksQuery(
    string? Referencia,
    string? Descricao,
    string? Familia,
    bool? Inactivo,
    int Page,
    int PageSize) : IRequest<GetAllStocksResultDTO>;

public sealed class GetAllStocksResultDTO
{
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public List<StockOutputDTO> Items { get; set; } = new();
}
