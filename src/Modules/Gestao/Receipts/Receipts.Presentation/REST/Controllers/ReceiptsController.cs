using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Receipts.Application.DTOs;
using Receipts.Application.Errors;
using Receipts.Application.Features.CreateReceipt;
using Receipts.Application.Features.GetAllReceipts;
using Receipts.Application.Features.GetReceiptById;
using Receipts.Application.Features.GetReceiptTypes;
using Receipts.Presentation.REST.Services;
using Shared.Kernel.Authorization;
using Shared.Kernel.DTOs;
using Shared.Kernel.Responses;

namespace Receipts.Presentation.REST.Controllers;

/// <summary>
/// Controller REST para o módulo de Recibos
/// </summary>
[ApiController]
[Route("api/receipts")]
[Produces("application/json")]
public sealed class ReceiptsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReceiptsLinkBuilder _linkBuilder;

    public ReceiptsController(IMediator mediator, IReceiptsLinkBuilder linkBuilder)
    {
        _mediator = mediator;
        _linkBuilder = linkBuilder;
    }

    /// <summary>
    /// Listar recibos com paginação e filtros.
    /// Por defeito as linhas não são incluídas; use <c>includeLines=true</c> para as obter.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("receipts-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAll(
        [FromQuery] decimal? ndoc = null,
        [FromQuery] decimal? rno = null,
        [FromQuery] decimal? reano = null,
        [FromQuery] decimal? no = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeLines = false,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAllReceiptsQuery(ndoc, rno, reano, no, page, pageSize, includeLines), ct);

        var totalPages = result.PageSize > 0
            ? (int)Math.Ceiling(result.TotalItems / (double)result.PageSize)
            : 1;
        if (totalPages < 1) totalPages = 1;

        var filterSuffix = BuildQueryString(ndoc, rno, reano, no, includeLines);
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
    /// Listar séries/tipos de recibo disponíveis (tabela tsre)
    /// </summary>
    [HttpGet("types")]
    [Authorize(Roles = AppRoles.ApiUser)]
    [EnableRateLimiting("receipts-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionResponseDTO>> GetTypes(CancellationToken ct = default)
    {
        var items = await _mediator.Send(new GetReceiptTypesQuery(), ct);
        var links = new List<HATEOASLink> { _linkBuilder.GetTypesLink() };
        return Ok(CollectionResponseDTO.FromPaged(items, items.Count, 1, items.Count == 0 ? 1 : items.Count, links));
    }

    /// <summary>
    /// Obter um recibo por chave composta (ndoc / rno / reano), incluindo as linhas
    /// </summary>
    [HttpGet("{ndoc}/{rno}/{reano}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<ReceiptOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<ReceiptOutputDTO>>> GetById(
        decimal ndoc, decimal rno, decimal reano, CancellationToken ct = default)
    {
        var receipt = await _mediator.Send(new GetReceiptByIdQuery(ndoc, rno, reano), ct);

        if (receipt is null)
            return NotFound(new { message = ReceiptsErrorCatalog.ReceiptNotFound.Description });

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetDetailByKeyLink(ndoc, rno, reano),
            _linkBuilder.GetListLink(1, 20)
        };

        return Ok(new SingleItemResponseDTO<ReceiptOutputDTO> { Item = receipt, Links = links });
    }

    /// <summary>
    /// Criar um Recibo via PHC WEB (EmitirRecibo)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("receipts-create")]
    [ProducesResponseType(typeof(CreateReceiptResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateReceiptResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CreateReceiptResponseDTO), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(CreateReceiptResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateReceiptResponseDTO>> Create(
        [FromBody] CreateReceiptInputDTO input,
        CancellationToken ct = default)
    {
        try
        {
            var createdBy = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.Identity?.Name
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(new CreateReceiptCommand(input, createdBy), ct);

            var re = result.Documents.Receipt;
            var links = new List<HATEOASLink> { _linkBuilder.GetListLink(1, 20) };

            if (re is not null)
                links.Insert(0, _linkBuilder.GetDetailByKeyLink(re.DocTypeId, re.ReceiptNumber, re.Year));

            return StatusCode(StatusCodes.Status201Created,
                CreateReceiptResponseDTO.Success(result, links));
        }
        catch (ValidationException ex)
        {
            var msg = ex.Errors.FirstOrDefault()?.ErrorMessage ?? ex.Message;
            return BadRequest(CreateReceiptResponseDTO.Error(ReceiptsErrorCatalog.ValidationError.Code, msg));
        }
        catch (ReceiptsModuleException ex)
        {
            var links = ex.ErrorCode == ReceiptsErrorCatalog.InvalidReceiptType.Code
                ? new List<HATEOASLink> { _linkBuilder.GetTypesLink() }
                : null;

            return UnprocessableEntity(CreateReceiptResponseDTO.Error(ex.ErrorCode, ex.Message, links));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateReceiptResponseDTO.Error("RE999", ex.Message));
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    private static string BuildQueryString(
        decimal? ndoc, decimal? rno, decimal? reano, decimal? no, bool includeLines)
    {
        var parts = new List<string>();
        if (ndoc.HasValue)   parts.Add($"ndoc={ndoc}");
        if (rno.HasValue)    parts.Add($"rno={rno}");
        if (reano.HasValue)  parts.Add($"reano={reano}");
        if (no.HasValue)     parts.Add($"no={no}");
        if (includeLines)    parts.Add("includeLines=true");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }
}
