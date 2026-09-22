using Dossiers.Application.DTOs;
using Dossiers.Application.Errors;
using Dossiers.Application.Features.CreateDossier;
using Dossiers.Application.Features.CreateDossiersBulk;
using Dossiers.Application.Features.DeleteDossierById;
using Dossiers.Application.Features.GetAllDossiers;
using Dossiers.Application.Features.GetDossierById;
using Dossiers.Application.Features.GetDossierTypes;
using Dossiers.Presentation.REST.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Kernel.Authorization;
using Shared.Kernel.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Dossiers.Presentation.REST.Controllers;

[ApiController]
[Route("api/dossiers")]
[Produces("application/json")]
public sealed class DossiersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDossiersLinkBuilder _linkBuilder;

    public DossiersController(IMediator mediator, IDossiersLinkBuilder linkBuilder)
    {
        _mediator = mediator;
        _linkBuilder = linkBuilder;
    }

    /// <summary>
    /// Listar dossiers com paginação e filtros. Por defeito as linhas não são incluídas; use <c>includeLinhas=true</c> para as obter.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("dossiers-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAll(
        [FromQuery] decimal? ndos = null,
        [FromQuery] string? nmdos = null,
        [FromQuery] decimal? obrano = null,
        [FromQuery] decimal? boano = null,
        [FromQuery] decimal? no = null,
        [FromQuery] decimal? estab = null,
        [FromQuery] string? nome = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeLines = false,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllDossiersQuery(
            ndos,
            nmdos,
            obrano,
            boano,
            no,
            estab,
            nome,
            page,
            pageSize,
            includeLines), ct);

        var totalPages = result.PageSize > 0
            ? (int)Math.Ceiling(result.TotalItems / (double)result.PageSize)
            : 1;

        if (totalPages < 1) totalPages = 1;

        var filterSuffix = BuildQueryString(ndos, nmdos, obrano, boano, no, estab, nome, includeLines);
        if (filterSuffix.StartsWith("?")) filterSuffix = "&" + filterSuffix[1..];

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetListLink(result.CurrentPage, result.PageSize, filterSuffix),
            _linkBuilder.GetLastLink(totalPages, result.PageSize, filterSuffix),
            _linkBuilder.GetCreateLink()
        };

        if (result.CurrentPage < totalPages)
        {
            links.Insert(1, _linkBuilder.GetNextLink(result.CurrentPage + 1, result.PageSize, filterSuffix));
        }

        var response = CollectionResponseDTO.FromPaged(result.Items, result.TotalItems, result.CurrentPage, result.PageSize, links);
        return Ok(response);
    }

    /// <summary>
    /// Obter tipos de dossier
    /// </summary>
    [HttpGet("types")]
    [Authorize(Roles = AppRoles.ApiUser)]
    [EnableRateLimiting("dossiers-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetTypes(CancellationToken ct = default)
    {
        var items = await _mediator.Send(new GetDossierTypesQuery(), ct);
        var links = new List<HATEOASLink> { _linkBuilder.GetTypesLink() };

        return Ok(CollectionResponseDTO.FromPaged(items, items.Count, 1, items.Count == 0 ? 1 : items.Count, links));
    }

    /// <summary>
    /// Criar um dossier
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("dossiers-create")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Create(
        [FromBody] Dossiers.Application.DTOs.CreateDossierInputDTO dto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.Identity?.Name
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        try
        {
            var created = await _mediator.Send(new CreateDossierCommand(dto, userId), ct);

            var links = new List<HATEOASLink>
            {
                _linkBuilder.GetDetailByKeyLink(created.Ndos, created.Obrano, created.Boano),
                _linkBuilder.GetListLink(1, 20)
            };

            var response = new PaginatedResponseDTO(DossiersErrorCatalog.Success.Code, "Dossier criado com sucesso", new[] { (object)created }, links);
            return CreatedAtAction(nameof(GetByKey), new { ndos = created.Ndos, obrano = created.Obrano, boano = created.Boano }, response);
        }
        catch (DossiersModuleException ex)
        {
            var links = BuildErrorLinks(ex.Code);
            return BadRequest(new PaginatedResponseDTO(ex.Code, ex.Message, null, links));
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var msg = first?.ErrorMessage ?? ex.Message;
            var code = !string.IsNullOrWhiteSpace(first?.ErrorCode) && first!.ErrorCode.StartsWith("BO")
                ? first.ErrorCode
                : DossiersErrorCatalog.ValidationError.Code;
            return BadRequest(PaginatedResponseDTO.Error(code, msg));
        }
    }

    /// <summary>
    /// Criar múltiplos dossiers em lote
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("dossiers-bulk-create")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> CreateBulk(
        [FromBody] Dossiers.Application.DTOs.CreateDossiersBulkInputDTO dto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.Identity?.Name
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        try
        {
            var bulkResult = await _mediator.Send(new CreateDossiersBulkCommand(dto, userId), ct);

            var links = new List<HATEOASLink>
            {
                _linkBuilder.GetListLink(1, 20)
            };

            var response = new PaginatedResponseDTO(
                bulkResult.Code,
                bulkResult.Message,
                bulkResult.Items.Cast<object>().ToList(),
                links);

            // Se todos falharam, retorna 400; se todos passou, 201; senão 206 (partial)
            if (bulkResult.FailureCount > 0 && bulkResult.SuccessCount == 0)
            {
                return BadRequest(response);
            }

            return bulkResult.SuccessCount > 0
                ? CreatedAtAction(nameof(GetAll), response)
                : BadRequest(response);
        }
        catch (DossiersModuleException ex)
        {
            var links = BuildErrorLinks(ex.Code);
            return BadRequest(new PaginatedResponseDTO(ex.Code, ex.Message, null, links));
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var msg = first?.ErrorMessage ?? ex.Message;
            return BadRequest(PaginatedResponseDTO.Error(DossiersErrorCatalog.ValidationError.Code, msg));
        }
    }

    /// <summary>
    /// Obter dossier pela chave composta (ndos, obrano, boano)
    /// </summary>
    [HttpGet("{ndos:decimal}/{obrano:decimal}/{boano:decimal}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("dossiers-query")]
    [ProducesResponseType(typeof(SingleDossierResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SingleDossierResponseDTO>> GetByKey(decimal ndos, decimal obrano, decimal boano, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDossierByIdQuery(ndos, obrano, boano), ct);

        if (result is null)
        {
            return NotFound(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));
        }

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetDetailByKeyLink(ndos, obrano, boano),
            _linkBuilder.GetListLink(1, 20),
            _linkBuilder.GetDeleteByKeyLink(ndos, obrano, boano)
        };

        return Ok(new SingleDossierResponseDTO { Item = result, Links = links });
    }

    /// <summary>
    /// Eliminar dossier pela chave composta (ndos, obrano, boano)
    /// </summary>
    [HttpDelete("{ndos:decimal}/{obrano:decimal}/{boano:decimal}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("dossiers-delete")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> DeleteByKey(decimal ndos, decimal obrano, decimal boano, CancellationToken ct = default)
    {
        var deleted = await _mediator.Send(new DeleteDossierByIdCommand(ndos, obrano, boano), ct);
        if (!deleted)
        {
            return NotFound(PaginatedResponseDTO.Error(
                DossiersErrorCatalog.DossierNotFound.Code,
                DossiersErrorCatalog.DossierNotFound.Description));
        }

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetListLink(1, 20)
        };

        return Ok(new PaginatedResponseDTO(DossiersErrorCatalog.Success.Code, "Dossier eliminado com sucesso", null, links));
    }

    private static string BuildQueryString(
        decimal? ndos,
        string? nmdos,
        decimal? obrano,
        decimal? boano,
        decimal? no,
        decimal? estab,
        string? nome,
        bool includeLinhas = false)
    {
        var parts = new List<string>();
        if (ndos.HasValue) parts.Add($"ndos={ndos.Value}");
        if (!string.IsNullOrWhiteSpace(nmdos)) parts.Add($"nmdos={Uri.EscapeDataString(nmdos)}");
        if (obrano.HasValue) parts.Add($"obrano={obrano.Value}");
        if (boano.HasValue) parts.Add($"boano={boano.Value}");
        if (no.HasValue) parts.Add($"no={no.Value}");
        if (estab.HasValue) parts.Add($"estab={estab.Value}");
        if (!string.IsNullOrWhiteSpace(nome)) parts.Add($"nome={Uri.EscapeDataString(nome)}");
        if (includeLinhas) parts.Add("includeLinhas=true");

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private IEnumerable<HATEOASLink>? BuildErrorLinks(string code)
    {
        if (code == DossiersErrorCatalog.ClientNotFound.Code)
        {
            return new[] { _linkBuilder.GetClientsListLink() };
        }

        if (code == DossiersErrorCatalog.InvalidTipoDossier.Code)
        {
            return new[] { _linkBuilder.GetTypesLink() };
        }

        if (code == DossiersErrorCatalog.ReferenceNotFound.Code)
        {
            return new[] { _linkBuilder.GetStocksListLink() };
        }

        if (code == DossiersErrorCatalog.InvalidCurrency.Code)
        {
            return new[] { _linkBuilder.GetCurrenciesListLink() };
        }

        if (code == DossiersErrorCatalog.InvalidVatTable.Code)
        {
            return new[] { _linkBuilder.GetVatTablesListLink() };
        }

        return null;
    }
}
