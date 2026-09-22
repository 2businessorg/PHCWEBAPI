using Invoices.Application.DTOs;
using MediatR;

namespace Invoices.Application.Features.GetAllInvoices;

/// <summary>
/// Query para listar faturas com paginação e filtros opcionais.
/// </summary>
public record GetAllInvoicesQuery(
    int? Ndoc,
    int? InvoiceNumber,
    int? Year,
    int? ClientNumber,
    int Page = 1,
    int PageSize = 20,
    bool IncludeLines = false
) : IRequest<GetAllInvoicesResultDTO>;
