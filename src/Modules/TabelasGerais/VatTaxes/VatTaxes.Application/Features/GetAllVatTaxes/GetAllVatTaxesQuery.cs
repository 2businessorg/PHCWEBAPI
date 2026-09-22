using MediatR;
using VatTaxes.Application.DTOs;

namespace VatTaxes.Application.Features.GetAllVatTaxes;

/// <summary>
/// Query para obter todas as taxas de IVA com paginação e filtros
/// </summary>
public sealed record GetAllVatTaxesQuery(
    int Page = 1,
    int PageSize = 50,
    int? Code = null,
    decimal? Rate = null) : IRequest<GetAllVatTaxesResultDTO>;
