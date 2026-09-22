using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Kernel.Authorization;
using Shared.Kernel.Responses;
using Stocks.Application.DTOs;
using Stocks.Application.Errors;
using Stocks.Application.Features.CreateStock;
using Stocks.Application.Features.CreateStocksBulk;
using Stocks.Application.Features.DeleteStock;
using Stocks.Application.Features.GetAllStocks;
using Stocks.Application.Features.GetStockByRef;
using Stocks.Application.Features.GetAllBatches;
using Stocks.Application.Features.GetStockBatchesByReference;
using Stocks.Application.Features.GetStockByWarehouse;
using Stocks.Application.Features.GetAllWarehouses;
using Stocks.Application.Features.UpdateStock;
using Stocks.Presentation.REST.Services;
using System.Security.Claims;

namespace Stocks.Presentation.REST.Controllers;

[ApiController]
[Route("api/stocks")]
[Produces("application/json")]
public sealed class StocksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStocksLinkBuilder _linkBuilder;

    public StocksController(IMediator mediator, IStocksLinkBuilder linkBuilder)
    {
        _mediator = mediator;
        _linkBuilder = linkBuilder;
    }

    /// <summary>
    /// Listar todos os stocks com paginação e filtros.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAll(
        [FromQuery] string? referencia = null,
        [FromQuery] string? descricao = null,
        [FromQuery] string? familia = null,
        [FromQuery] bool? inactivo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAllStocksQuery(referencia, descricao, familia, inactivo, page, pageSize), ct);

        var totalPages = result.PageSize > 0
            ? (int)Math.Ceiling(result.TotalItems / (double)result.PageSize)
            : 1;

        if (totalPages < 1) totalPages = 1;

        var filterSuffix = BuildQueryString(referencia, descricao, familia, inactivo);
        if (filterSuffix.StartsWith("?")) filterSuffix = "&" + filterSuffix[1..];

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetSelfPaginationLink(result.CurrentPage, result.PageSize, filterSuffix),
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
    /// Listar todos os batches (lotes) com paginação e filtros.
    /// </summary>
    [HttpGet("batches")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAllBatches(
        [FromQuery] string? referencia = null,
        [FromQuery] string? lote = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAllBatchesQuery(referencia, lote, page, pageSize), ct);

        var totalPages = result.PageSize > 0
            ? (int)Math.Ceiling(result.TotalItems / (double)result.PageSize)
            : 1;

        if (totalPages < 1) totalPages = 1;

        var filterSuffix = BuildBatchQueryString(referencia, lote);
        if (filterSuffix.StartsWith("?")) filterSuffix = "&" + filterSuffix[1..];

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetSelfPaginationLink(result.CurrentPage, result.PageSize, filterSuffix),
            _linkBuilder.GetLastLink(totalPages, result.PageSize, filterSuffix)
        };

        if (result.CurrentPage < totalPages)
        {
            links.Insert(1, _linkBuilder.GetNextLink(result.CurrentPage + 1, result.PageSize, filterSuffix));
        }

        var response = CollectionResponseDTO.FromPaged(result.Items, result.TotalItems, result.CurrentPage, result.PageSize, links);
        return Ok(response);
    }

    /// <summary>
    /// Obter stock pela referência.
    /// </summary>
    [HttpGet("{referencia}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-query")]
    [ProducesResponseType(typeof(SingleStockResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SingleStockResponseDTO>> GetByRef(string referencia, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetStockByRefQuery(referencia), ct);

        if (result is null)
        {
            return NotFound(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));
        }

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetDetailLink(referencia),
            _linkBuilder.GetListLink(),
            _linkBuilder.GetUpdateLink(referencia),
            _linkBuilder.GetDeleteLink(referencia)
        };

        return Ok(new SingleStockResponseDTO { Item = result, Links = links });
    }

    /// <summary>
    /// Listar todos os armazéns.
    /// </summary>
    [HttpGet("warehouses")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAllWarehouses(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllWarehousesQuery(), ct);

        var href = "/api/stocks/warehouses";
        var links = new List<HATEOASLink>
        {
            new HATEOASLink("self", href, "GET")
        };

        return Ok(CollectionResponseDTO.FromPaged(
            result.Items.Cast<object>(),
            result.Metadata.TotalItems,
            1,
            50,
            links));
    }

    /// <summary>
    /// Obter stocks de um lote específico por armazém.
    /// </summary>
    [HttpGet("{referencia}/batches")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetStockBatchesByReference(
        string referencia,
        [FromQuery] string? lote = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetStockBatchesByReferenceQuery(referencia, lote), ct);

        if (result.Items.Count == 0)
        {
            return NotFound(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));
        }

        var href = $"/api/stocks/{referencia}/batches{(string.IsNullOrEmpty(lote) ? string.Empty : $"?batch={lote}")}";
        var links = new List<HATEOASLink>
        {
            new HATEOASLink("self", href, "GET")
        };

        return Ok(CollectionResponseDTO.FromPaged(
            result.Items.Cast<object>(), 
            result.Metadata.TotalItems, 
            1, 
            50, 
            links));
    }

    /// <summary>
    /// Obter stock agregado por armazém (sem especificar lote).
    /// </summary>
    [HttpGet("{referencia}/warehouses")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetStockByWarehouse(
        string referencia,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetStockByWarehouseQuery(referencia), ct);

        if (result.Items.Count == 0)
        {
            return NotFound(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));
        }

        var href = $"/api/stocks/{referencia}/warehouses";
        var links = new List<HATEOASLink>
        {
            new HATEOASLink("self", href, "GET")
        };

        return Ok(CollectionResponseDTO.FromPaged(
            result.Items.Cast<object>(), 
            result.Metadata.TotalItems, 
            1, 
            50, 
            links));
    }

    /// <summary>
    /// Criar novo stock.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-create")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Create(
        [FromBody] CreateStockInputDTO dto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.Identity?.Name
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        try
        {
            var result = await _mediator.Send(new CreateStockCommand(dto, userId), ct);

            var links = new List<HATEOASLink>
            {
                _linkBuilder.GetDetailLink(result.Referencia),
                _linkBuilder.GetListLink()
            };

            var response = new PaginatedResponseDTO(
                StocksErrorCatalog.Success.Code,
                "Stock criado com sucesso",
                new[] { (object)result },
                links);

            return CreatedAtAction(nameof(GetByRef), new { referencia = result.Referencia }, response);
        }
        catch (StocksModuleException ex)
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
                : StocksErrorCatalog.InvalidReference.Code;
            return BadRequest(PaginatedResponseDTO.Error(code, msg));
        }
    }

    /// <summary>
    /// Criar múltiplos stocks em lote.
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-bulk-create")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> CreateBulk(
        [FromBody] CreateStockBulkInputDTO dto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        try
        {
            var bulkResult = await _mediator.Send(new CreateStocksBulkCommand(dto, userId), ct);

            var links = new List<HATEOASLink>
            {
                _linkBuilder.GetListLink()
            };

            var response = new PaginatedResponseDTO(
                bulkResult.Code,
                bulkResult.Message,
                bulkResult.Items.Cast<object>().ToList(),
                links);

            if (bulkResult.FailureCount > 0 && bulkResult.SuccessCount == 0)
            {
                return BadRequest(response);
            }

            return bulkResult.SuccessCount > 0
                ? CreatedAtAction(nameof(GetAll), response)
                : BadRequest(response);
        }
        catch (StocksModuleException ex)
        {
            var links = BuildErrorLinks(ex.Code);
            return BadRequest(new PaginatedResponseDTO(ex.Code, ex.Message, null, links));
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var msg = first?.ErrorMessage ?? ex.Message;
            return BadRequest(PaginatedResponseDTO.Error(StocksErrorCatalog.ValidationError.Code, msg));
        }
    }

    /// <summary>
    /// Atualizar stock pela referência.
    /// </summary>
    [HttpPatch("{referencia}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("stocks-update")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Update(
        string referencia,
        [FromBody] UpdateStockInputDTO dto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        try
        {
            var result = await _mediator.Send(new UpdateStockCommand(referencia, dto, userId), ct);

            if (result is null)
            {
                return NotFound(PaginatedResponseDTO.Error(
                    StocksErrorCatalog.StockNotFound.Code,
                    StocksErrorCatalog.StockNotFound.Description));
            }

            var links = new List<HATEOASLink>
            {
                _linkBuilder.GetDetailLink(referencia),
                _linkBuilder.GetListLink()
            };

            return Ok(new PaginatedResponseDTO(
                StocksErrorCatalog.Success.Code,
                "Stock atualizado com sucesso",
                new[] { (object)result },
                links));
        }
        catch (StocksModuleException ex)
        {
            var links = BuildErrorLinks(ex.Code);
            return BadRequest(new PaginatedResponseDTO(ex.Code, ex.Message, null, links));
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var msg = first?.ErrorMessage ?? ex.Message;
            return BadRequest(PaginatedResponseDTO.Error(StocksErrorCatalog.ValidationError.Code, msg));
        }
    }

    /// <summary>
    /// Eliminar stock pela referência.
    /// </summary>
    [HttpDelete("{referencia}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("stocks-delete")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Delete(string referencia, CancellationToken ct = default)
    {
        var deleted = await _mediator.Send(new DeleteStockCommand(referencia), ct);

        if (!deleted)
        {
            return NotFound(PaginatedResponseDTO.Error(
                StocksErrorCatalog.StockNotFound.Code,
                StocksErrorCatalog.StockNotFound.Description));
        }

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetListLink()
        };

        return Ok(new PaginatedResponseDTO(
            StocksErrorCatalog.Success.Code,
            "Stock eliminado com sucesso",
            null,
            links));
    }

    private static string BuildQueryString(string? referencia, string? descricao, string? familia, bool? inactivo)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(referencia)) parts.Add($"referencia={Uri.EscapeDataString(referencia)}");
        if (!string.IsNullOrWhiteSpace(descricao)) parts.Add($"descricao={Uri.EscapeDataString(descricao)}");
        if (!string.IsNullOrWhiteSpace(familia)) parts.Add($"familia={Uri.EscapeDataString(familia)}");
        if (inactivo.HasValue) parts.Add($"inactivo={inactivo.Value.ToString().ToLowerInvariant()}");

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private string BuildBatchQueryString(string? referencia, string? lote)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(referencia)) parts.Add($"referencia={Uri.EscapeDataString(referencia)}");
        if (!string.IsNullOrWhiteSpace(lote)) parts.Add($"lote={Uri.EscapeDataString(lote)}");

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private IEnumerable<HATEOASLink>? BuildErrorLinks(string code)
    {
        if (code == StocksErrorCatalog.StockNotFound.Code)
        {
            return new[] { _linkBuilder.GetListLink() };
        }

        return null;
    }
}
