using MediatR;
using Treasury.Application.DTOs;

namespace Treasury.Application.Features.GetReconciliationMovements;

/// <summary>
/// Query for unreconciled imported bank movements (BR) and treasury account movements (BA).
/// </summary>
/// <param name="AccountName">Treasury account display name (<c>bl.banco</c>).</param>
/// <param name="DateFrom">Inclusive start date.</param>
/// <param name="DateTo">Inclusive end date.</param>
/// <param name="Page">1-based page. Reserved for later paging of large windows.</param>
/// <param name="PageSize">Page size. Defaults to a large window suitable for the POC.</param>
public record GetReconciliationMovementsQuery(
    string AccountName,
    DateOnly DateFrom,
    DateOnly DateTo,
    int Page = 1,
    int PageSize = 500
) : IRequest<ReconciliationMovementsOutputDTO>;
