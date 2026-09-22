using Advances.Application.DTOs;
using Advances.Application.Errors;
using Advances.Application.Features.CreateAdvance;
using Advances.Application.Features.GetAllAdvances;
using Advances.Application.Features.GetAdvanceById;
using Advances.Application.Features.GetAdvanceTypes;
using Advances.Presentation.REST.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Kernel.Authorization;
using Shared.Kernel.DTOs;
using Shared.Kernel.Responses;

namespace Advances.Presentation.REST.Controllers;

/// <summary>
/// Controller REST para o módulo de Adiantamentos (RD)
/// </summary>
[ApiController]
[Route("api/advances")]
[Produces("application/json")]
public sealed class AdvancesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAdvancesLinkBuilder _linkBuilder;

    public AdvancesController(IMediator mediator, IAdvancesLinkBuilder linkBuilder)
    {
        _mediator = mediator;
        _linkBuilder = linkBuilder;
    }

    /// <summary>
    /// Listar adiantamentos com paginação e filtros opcionais.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("advances-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAll(
        [FromQuery] decimal? ndoc = null,
        [FromQuery] decimal? rno = null,
        [FromQuery] decimal? rdano = null,
        [FromQuery] decimal? no = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAllAdvancesQuery(ndoc, rno, rdano, no, page, pageSize), ct);

        var totalPages = result.PageSize > 0
            ? (int)Math.Ceiling(result.TotalItems / (double)result.PageSize)
            : 1;
        if (totalPages < 1) totalPages = 1;

        var filterSuffix = BuildQueryString(ndoc, rno, rdano, no);
        if (filterSuffix.StartsWith("?")) filterSuffix = "&" + filterSuffix[1..];

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetListLink(result.CurrentPage, result.PageSize, filterSuffix),
            _linkBuilder.GetLastLink(totalPages, result.PageSize, filterSuffix),
            _linkBuilder.GetCreateLink()
        };

        if (result.CurrentPage < totalPages)
            links.Insert(1, _linkBuilder.GetNextLink(result.CurrentPage + 1, result.PageSize, filterSuffix));

        var response = CollectionResponseDTO.FromPaged(result.Items, result.TotalItems, result.CurrentPage, result.PageSize, links);
        return Ok(response);
    }

    /// <summary>
    /// Listar séries/tipos de adiantamento disponíveis (tabela tsrd)
    /// </summary>
    [HttpGet("types")]
    [Authorize(Roles = AppRoles.ApiUser)]
    [EnableRateLimiting("advances-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionResponseDTO>> GetTypes(CancellationToken ct = default)
    {
        var items = await _mediator.Send(new GetAdvanceTypesQuery(), ct);
        var links = new List<HATEOASLink> { _linkBuilder.GetTypesLink() };
        return Ok(CollectionResponseDTO.FromPaged(items, items.Count, 1, items.Count == 0 ? 1 : items.Count, links));
    }

    /// <summary>
    /// Obter um adiantamento por chave composta (ndoc / rno / rdano)
    /// </summary>
    [HttpGet("{ndoc}/{rno}/{rdano}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<AdvanceOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<AdvanceOutputDTO>>> GetById(
        decimal ndoc, decimal rno, decimal rdano, CancellationToken ct = default)
    {
        var advance = await _mediator.Send(new GetAdvanceByIdQuery(ndoc, rno, rdano), ct);

        if (advance is null)
            return NotFound(new { message = AdvancesErrorCatalog.AdvanceNotFound.Description });

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetDetailByKeyLink(ndoc, rno, rdano),
            _linkBuilder.GetListLink(1, 20)
        };

        return Ok(new SingleItemResponseDTO<AdvanceOutputDTO> { Item = advance, Links = links });
    }

    /// <summary>
    /// Criar um Adiantamento via PHC WEB (script insertRdAPI)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("advances-create")]
    [ProducesResponseType(typeof(CreateAdvanceResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateAdvanceResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CreateAdvanceResponseDTO), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(CreateAdvanceResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateAdvanceResponseDTO>> Create(
        [FromBody] CreateAdvanceInputDTO input,
        CancellationToken ct = default)
    {
        try
        {
            var createdBy = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.Identity?.Name
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(new CreateAdvanceCommand(input, createdBy), ct);

            var links = new List<HATEOASLink>
            {
                _linkBuilder.GetDetailByKeyLink(result.Ndoc, result.Rno, result.Rdano),
                _linkBuilder.GetListLink(1, 20)
            };

            return StatusCode(StatusCodes.Status201Created,
                CreateAdvanceResponseDTO.Success(result, links));
        }
        catch (ValidationException ex)
        {
            var msg = ex.Errors.FirstOrDefault()?.ErrorMessage ?? ex.Message;
            return BadRequest(CreateAdvanceResponseDTO.Error(AdvancesErrorCatalog.ValidationError.Code, msg));
        }
        catch (AdvancesModuleException ex)
        {
            var links = ex.ErrorCode == AdvancesErrorCatalog.InvalidAdvanceType.Code
                ? new List<HATEOASLink> { _linkBuilder.GetTypesLink() }
                : null;

            return UnprocessableEntity(CreateAdvanceResponseDTO.Error(ex.ErrorCode, ex.Message, links));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateAdvanceResponseDTO.Error("AD999", ex.Message));
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    private static string BuildQueryString(decimal? ndoc, decimal? rno, decimal? rdano, decimal? no)
    {
        var parts = new List<string>();
        if (ndoc.HasValue)  parts.Add($"ndoc={ndoc}");
        if (rno.HasValue)   parts.Add($"rno={rno}");
        if (rdano.HasValue) parts.Add($"rdano={rdano}");
        if (no.HasValue)    parts.Add($"no={no}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
