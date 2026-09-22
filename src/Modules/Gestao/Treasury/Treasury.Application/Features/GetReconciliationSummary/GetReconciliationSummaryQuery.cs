using MediatR;
using Treasury.Application.DTOs;

namespace Treasury.Application.Features.GetReconciliationSummary;

/// <summary>
/// Query for unreconciled BR/BA counts and money totals without line items.
/// </summary>
public record GetReconciliationSummaryQuery(
    string AccountName,
    DateOnly DateFrom,
    DateOnly DateTo
) : IRequest<ReconciliationSummaryOutputDTO>;
