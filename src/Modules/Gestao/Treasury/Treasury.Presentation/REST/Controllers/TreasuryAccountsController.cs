using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Kernel.Authorization;
using Shared.Kernel.DTOs;
using Shared.Kernel.Responses;
using Treasury.Application.DTOs;
using Treasury.Application.Features.GetTreasuryAccount;
using Treasury.Application.Features.GetTreasuryAccounts;
using Treasury.Presentation.REST.Services;

namespace Treasury.Presentation.REST.Controllers;

/// <summary>
/// REST API for PHC treasury accounts (<c>bl</c>).
/// </summary>
[ApiController]
[Route("api/treasury/accounts")]
[Produces("application/json")]
public sealed class TreasuryAccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITreasuryLinkBuilder _linkBuilder;

    public TreasuryAccountsController(IMediator mediator, ITreasuryLinkBuilder linkBuilder)
    {
        _mediator = mediator;
        _linkBuilder = linkBuilder;
    }

    /// <summary>
    /// Lists treasury accounts. Use the exact <c>name</c> value as <c>accountName</c> on other treasury endpoints.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("treasury-query")]
    [ProducesResponseType(typeof(SingleItemResponseDTO<TreasuryAccountListOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SingleItemResponseDTO<TreasuryAccountListOutputDTO>>> List(
        [FromQuery] string? nameContains = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetTreasuryAccountsQuery(nameContains, includeInactive, page, pageSize),
            ct);

        return Ok(new SingleItemResponseDTO<TreasuryAccountListOutputDTO>
        {
            Item = result,
            Links = new List<HATEOASLink>
            {
                _linkBuilder.GetAccountsLink(nameContains, includeInactive, page, pageSize)
            }
        });
    }

    /// <summary>
    /// Returns one treasury account by display name (<c>bl.banco</c>).
    /// </summary>
    [HttpGet("{accountName}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("treasury-query")]
    [ProducesResponseType(typeof(SingleItemResponseDTO<TreasuryAccountOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<TreasuryAccountOutputDTO>>> GetByName(
        [FromRoute] string accountName,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTreasuryAccountQuery(accountName), ct);

        return Ok(new SingleItemResponseDTO<TreasuryAccountOutputDTO>
        {
            Item = result,
            Links = new List<HATEOASLink>
            {
                _linkBuilder.GetAccountLink(result.Name),
                _linkBuilder.GetAccountsLink()
            }
        });
    }
}
