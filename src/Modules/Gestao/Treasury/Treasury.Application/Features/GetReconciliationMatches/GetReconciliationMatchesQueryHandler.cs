using MediatR;
using Treasury.Application.DTOs;
using Treasury.Application.Features.GetReconciliationMovements;
using Treasury.Application.Matching;

namespace Treasury.Application.Features.GetReconciliationMatches;

/// <summary>
/// Loads unreconciled movements and returns suggested combinations.
/// </summary>
public class GetReconciliationMatchesQueryHandler
    : IRequestHandler<GetReconciliationMatchesQuery, ReconciliationMatchesOutputDTO>
{
    private readonly IMediator _mediator;
    private readonly IReconciliationMatcher _matcher;

    public GetReconciliationMatchesQueryHandler(IMediator mediator, IReconciliationMatcher matcher)
    {
        _mediator = mediator;
        _matcher = matcher;
    }

    public async Task<ReconciliationMatchesOutputDTO> Handle(
        GetReconciliationMatchesQuery request,
        CancellationToken cancellationToken)
    {
        var movements = await _mediator.Send(
            new GetReconciliationMovementsQuery(
                request.AccountName,
                request.DateFrom,
                request.DateTo,
                Page: 1,
                PageSize: 1000),
            cancellationToken);

        var combinations = _matcher.Match(movements.BankMovements, movements.TreasuryMovements);

        return new ReconciliationMatchesOutputDTO
        {
            Account = movements.Account,
            Period = movements.Period,
            BankMovementCount = movements.BankMovementTotalCount,
            TreasuryMovementCount = movements.TreasuryMovementTotalCount,
            MatchedCount = combinations.Count(c => c.Status == ReconciliationMatchStatuses.Matched),
            PartialMatchedCount = combinations.Count(c => c.Status == ReconciliationMatchStatuses.PartialMatched),
            UnmatchedBankCount = combinations
                .Where(c => c.Status == ReconciliationMatchStatuses.Unmatched)
                .Sum(c => c.BankCount),
            UnmatchedTreasuryCount = combinations
                .Where(c => c.Status == ReconciliationMatchStatuses.Unmatched)
                .Sum(c => c.TreasuryCount),
            Combinations = combinations
        };
    }
}
