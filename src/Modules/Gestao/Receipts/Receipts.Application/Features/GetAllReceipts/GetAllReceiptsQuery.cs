using MediatR;
using Receipts.Application.DTOs;

namespace Receipts.Application.Features.GetAllReceipts;

/// <summary>
/// Query para listar Recibos com filtros e paginação
/// </summary>
public record GetAllReceiptsQuery(
    decimal? Ndoc,
    decimal? Rno,
    decimal? Reano,
    decimal? No,
    int Page = 1,
    int PageSize = 20,
    bool IncludeLines = false
) : IRequest<GetAllReceiptsResultDTO>;
