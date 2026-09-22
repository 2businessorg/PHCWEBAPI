using MediatR;
using Treasury.Application.DTOs;

namespace Treasury.Application.Features.GetReconciliationMatches;

/// <summary>
/// Suggests MATCHED / PARTIAL MATCHED / UNMATCHED combinations for a period.
/// Read-only: does not write <c>reco</c>.
/// </summary>
public record GetReconciliationMatchesQuery(
    string AccountName,
    DateOnly DateFrom,
    DateOnly DateTo
) : IRequest<ReconciliationMatchesOutputDTO>;
