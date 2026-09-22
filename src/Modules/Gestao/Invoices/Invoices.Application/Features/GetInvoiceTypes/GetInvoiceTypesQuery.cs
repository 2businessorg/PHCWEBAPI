using MediatR;
using Invoices.Application.DTOs;

namespace Invoices.Application.Features.GetInvoiceTypes;

/// <summary>
/// Query para obter tipos de série de facturação
/// </summary>
public record GetInvoiceTypesQuery : IRequest<IReadOnlyList<InvoiceTypeOutputDTO>>;
