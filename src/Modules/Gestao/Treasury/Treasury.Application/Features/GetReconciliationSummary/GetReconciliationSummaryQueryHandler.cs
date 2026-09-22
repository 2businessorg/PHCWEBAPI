using MediatR;
using Treasury.Application.DTOs;
using Treasury.Application.Features.GetReconciliationMovements;
using Treasury.Application.Mappings;

namespace Treasury.Application.Features.GetReconciliationSummary;

/// <summary>
/// Projects the full reconciliation query into counts and money totals only.
/// </summary>
public class GetReconciliationSummaryQueryHandler
    : IRequestHandler<GetReconciliationSummaryQuery, ReconciliationSummaryOutputDTO>
{
    private readonly IMediator _mediator;

    public GetReconciliationSummaryQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ReconciliationSummaryOutputDTO> Handle(
        GetReconciliationSummaryQuery request,
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

        return ReconciliationSummaryMapper.From(movements);
    }
}
