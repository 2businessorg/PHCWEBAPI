using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Kernel.Authorization;
using Shared.Kernel.DTOs;
using Shared.Kernel.Responses;
using Treasury.Application.DTOs;
using Treasury.Application.Features.GetReconciliationMatches;
using Treasury.Application.Features.GetReconciliationMovements;
using Treasury.Application.Features.GetReconciliationSummary;
using Treasury.Presentation.REST.Services;

namespace Treasury.Presentation.REST.Controllers;

/// <summary>
/// REST API for treasury bank reconciliation (read-only).
/// </summary>
[ApiController]
[Route("api/treasury/bank-reconciliation")]
[Produces("application/json")]
public sealed class BankReconciliationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITreasuryLinkBuilder _linkBuilder;

    public BankReconciliationController(IMediator mediator, ITreasuryLinkBuilder linkBuilder)
    {
        _mediator = mediator;
        _linkBuilder = linkBuilder;
    }

    /// <summary>
    /// Returns unreconciled imported bank movements (BR) and treasury account movements (BA)
    /// for a treasury account and inclusive date range.
    /// </summary>
    [HttpGet("movements")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("treasury-query")]
    [ProducesResponseType(typeof(SingleItemResponseDTO<ReconciliationMovementsOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<ReconciliationMovementsOutputDTO>>> GetMovements(
        [FromQuery] string accountName,
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 500,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetReconciliationMovementsQuery(accountName, dateFrom, dateTo, page, pageSize),
            ct);

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetMovementsLink(accountName, dateFrom, dateTo, page, pageSize),
            _linkBuilder.GetSummaryLink(accountName, dateFrom, dateTo),
            _linkBuilder.GetMatchesLink(accountName, dateFrom, dateTo),
            _linkBuilder.GetAccountLink(accountName)
        };

        return Ok(new SingleItemResponseDTO<ReconciliationMovementsOutputDTO>
        {
            Item = result,
            Links = links
        });
    }

    /// <summary>
    /// Returns unreconciled BR/BA counts and money totals without movement line items.
    /// Prefer this endpoint when only aggregates are needed.
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("treasury-query")]
    [ProducesResponseType(typeof(SingleItemResponseDTO<ReconciliationSummaryOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<ReconciliationSummaryOutputDTO>>> GetSummary(
        [FromQuery] string accountName,
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetReconciliationSummaryQuery(accountName, dateFrom, dateTo),
            ct);

        return Ok(new SingleItemResponseDTO<ReconciliationSummaryOutputDTO>
        {
            Item = result,
            Links = new List<HATEOASLink>
            {
                _linkBuilder.GetSummaryLink(accountName, dateFrom, dateTo),
                _linkBuilder.GetMovementsLink(accountName, dateFrom, dateTo),
                _linkBuilder.GetMatchesLink(accountName, dateFrom, dateTo),
                _linkBuilder.GetAccountLink(accountName)
            }
        });
    }

    /// <summary>
    /// Suggests MATCHED / PARTIAL MATCHED / UNMATCHED combinations between BR and BA.
    /// Read-only proposal — does not write <c>reco</c>.
    /// </summary>
    [HttpGet("matches")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("treasury-query")]
    [ProducesResponseType(typeof(SingleItemResponseDTO<ReconciliationMatchesOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<ReconciliationMatchesOutputDTO>>> GetMatches(
        [FromQuery] string accountName,
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetReconciliationMatchesQuery(accountName, dateFrom, dateTo),
            ct);

        return Ok(new SingleItemResponseDTO<ReconciliationMatchesOutputDTO>
        {
            Item = result,
            Links = new List<HATEOASLink>
            {
                _linkBuilder.GetMatchesLink(accountName, dateFrom, dateTo),
                _linkBuilder.GetSummaryLink(accountName, dateFrom, dateTo),
                _linkBuilder.GetMovementsLink(accountName, dateFrom, dateTo),
                _linkBuilder.GetAccountLink(accountName)
            }
        });
    }
}
