using FluentValidation;
using Invoices.Application.DTOs;
using Invoices.Application.Features.CreateFatura;
using Invoices.Application.Features.DeleteInvoiceById;
using Invoices.Application.Features.GetAllInvoices;
using Invoices.Application.Features.GetInvoiceById;
using Invoices.Application.Features.GetInvoiceTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel.Authorization;
using Shared.Kernel.Responses;

namespace Invoices.Presentation.Controllers;

/// <summary>
/// Controlador para gestão de Faturas.
/// Endpoints para criar, listar, recuperar e eliminar faturas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obter tipos de série de facturação disponíveis.
    /// </summary>
    [HttpGet("types")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetTypes(CancellationToken ct = default)
    {
        var items = await _mediator.Send(new GetInvoiceTypesQuery(), ct);
        var links = new List<HATEOASLink> { new("self", "/api/invoices/types", "GET") };

        return Ok(CollectionResponseDTO.FromPaged(items, items.Count, 1, items.Count == 0 ? 1 : items.Count, links));
    }

    /// <summary>
    /// Lista faturas com filtros opcionais e paginação.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAll(
        [FromQuery] int? ndoc = null,
        [FromQuery] int? invoiceNumber = null,
        [FromQuery] int? year = null,
        [FromQuery] int? clientNumber = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeLines = false,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllInvoicesQuery(
            ndoc,
            invoiceNumber,
            year,
            clientNumber,
            page,
            pageSize,
            includeLines), ct);

        var links = new List<HATEOASLink>
        {
            new("self", $"/api/invoices?page={result.CurrentPage}&pageSize={result.PageSize}", "GET"),
            new("create", "/api/invoices", "POST")
        };

        var response = CollectionResponseDTO.FromPaged(
            result.Items,
            result.TotalItems,
            result.CurrentPage,
            result.PageSize,
            links);

        return Ok(response);
    }

    /// <summary>
    /// Recupera uma fatura específica pela chave composta (ndoc, invoiceNumber, year).
    /// </summary>
    [HttpGet("{ndoc:int}/{invoiceNumber:int}/{year:int}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleInvoiceResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleInvoiceResponseDTO>> GetByKey(
        int ndoc,
        int invoiceNumber,
        int year,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(ndoc, invoiceNumber, year), ct);
        if (result is null)
        {
            return NotFound(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));
        }

        var links = new List<HATEOASLink>
        {
            new("self", $"/api/invoices/{ndoc}/{invoiceNumber}/{year}", "GET"),
            new("list", "/api/invoices?page=1&pageSize=20", "GET"),
            new("delete", $"/api/invoices/{ndoc}/{invoiceNumber}/{year}", "DELETE")
        };

        return Ok(new SingleInvoiceResponseDTO
        {
            Item = result,
            Links = links
        });
    }

    /// <summary>
    /// Cria uma nova fatura.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(CreateInvoiceResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateInvoiceResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CreateInvoiceResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateInvoiceResponseDTO>> Create(
        [FromBody] CreateInvoiceInputDTO request,
        CancellationToken ct = default)
    {
        try
        {
            var createdBy = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.Identity?.Name
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var created = await _mediator.Send(new CreateFaturaCommand { Request = request, CreatedBy = createdBy }, ct);

            var links = new List<HATEOASLink>
            {
                new("self", $"/api/invoices/{created.Ndoc}/{created.Fno}/{created.Ftano}", "GET"),
                new("list", "/api/invoices?page=1&pageSize=20", "GET"),
                new("delete", $"/api/invoices/{created.Ndoc}/{created.Fno}/{created.Ftano}", "DELETE")
            };

            var response = CreateInvoiceResponseDTO.Success(created, links);

            return CreatedAtAction(nameof(GetByKey), new { ndoc = created.Ndoc, invoiceNumber = created.Fno, year = created.Ftano }, response);
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var msg = first?.ErrorMessage ?? ex.Message;
            return BadRequest(CreateInvoiceResponseDTO.Error("FT001", msg));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, CreateInvoiceResponseDTO.Error("FT012", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, CreateInvoiceResponseDTO.Error("FT999", ex.Message));
        }
    }

    /// <summary>
    /// Elimina uma fatura específica pela chave composta (ndoc, invoiceNumber, year).
    /// </summary>
    [HttpDelete("{ndoc:int}/{invoiceNumber:int}/{year:int}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResponseDTO>> DeleteByKey(
        int ndoc,
        int invoiceNumber,
        int year,
        CancellationToken ct = default)
    {
        var deleted = await _mediator.Send(new DeleteInvoiceByIdCommand(ndoc, invoiceNumber, year), ct);
        if (!deleted)
        {
            return NotFound(PaginatedResponseDTO.Error("FT002", "Fatura não encontrada"));
        }

        var links = new List<HATEOASLink>
        {
            new("list", "/api/invoices?page=1&pageSize=20", "GET")
        };

        return Ok(new PaginatedResponseDTO("0000", "Fatura eliminada com sucesso", null, links));
    }
}
