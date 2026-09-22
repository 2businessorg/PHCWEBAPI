using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Collections.Generic;
using VatTaxes.Application.DTOs;
using VatTaxes.Application.Errors;
using VatTaxes.Application.Features.GetAllVatTaxes;
using VatTaxes.Application.Features.GetVatTaxByTabiva;
using VatTaxes.Application.Features.UpdateVatTax;
using Shared.Kernel.Responses;
using Shared.Kernel.DTOs;
using Shared.Kernel.Authorization;

namespace VatTaxes.Presentation.REST.Controllers;

/// <summary>
/// Controller para gestão de Taxas de IVA (VAT Taxes)
/// </summary>
[ApiController]
[Route("api/vatTaxes")]
[Produces("application/json")]
public sealed class VatTaxesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VatTaxesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Listar todas as taxas de IVA com paginação e filtros
    /// </summary>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50)</param>
    /// <param name="code">Filtro por código da taxa (opcional)</param>
    /// <param name="rate">Filtro por percentagem de IVA (opcional)</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("vat-taxes-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? code = null,
        [FromQuery] decimal? rate = null,
        CancellationToken ct = default)
    {
        // Executar query com MediatR
        var query = new GetAllVatTaxesQuery(page, pageSize, code, rate);
        var result = await _mediator.Send(query, ct);
        
        // Converter resultado para CollectionResponseDTO
        var response = CollectionResponseDTO.FromPaged(
            result.Items,
            result.TotalItems,
            result.CurrentPage,
            result.PageSize
        );
        
        return Ok(response);
    }

    /// <summary>
    /// Obter taxa de IVA por código
    /// </summary>
    /// <param name="code">Código da taxa de IVA</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpGet("{code:int}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("vat-taxes-query")]
    [ProducesResponseType(typeof(SingleVatTaxResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SingleVatTaxResponseDTO>> GetByCode(int code, CancellationToken ct = default)
    {
        // Validar código
        if (code <= 0)
            return BadRequest(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));

        // Executar query com MediatR
        var query = new GetVatTaxByTabivaQuery(code);
        var result = await _mediator.Send(query, ct);

        // Retornar resultado ou NotFound
        if (result == null)
            return NotFound(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));

        var links = new List<HATEOASLink>
        {
            new HATEOASLink("self", $"/api/vatTaxes/{code}"),
            new HATEOASLink("list", "/api/vatTaxes")
        };

        return Ok(new SingleVatTaxResponseDTO { Item = result, Links = links });
    }

    /// <summary>
    /// Atualizar taxa de IVA (atualização parcial)
    /// </summary>
    /// <param name="code">Código da taxa de IVA a atualizar</param>
    /// <param name="dto">Dados a atualizar (rate, reference, description)</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpPatch("{code:int}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("vat-taxes-update")]
    [ProducesResponseType(typeof(SingleVatTaxResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SingleVatTaxResponseDTO>> Update(
        int code,
        [FromBody] UpdateVatTaxInputDTO dto,
        CancellationToken ct = default)
    {
        // Executar command com MediatR
        var updatedBy = User.Identity?.Name ?? "PHCAPI";
        var command = new UpdateVatTaxCommand(code, dto.Taxa, dto.Ref, dto.Design, updatedBy);
        var result = await _mediator.Send(command, ct);

        // Retornar resultado ou NotFound
        if (result == null)
            return NotFound(ResponseDTO.Error(new ResponseCodeDTO(VatTaxesErrorCatalog.VatTaxNotFound.Code, VatTaxesErrorCatalog.VatTaxNotFound.Description)));

        var links = new List<HATEOASLink>
        {
            new HATEOASLink("self", $"/api/vatTaxes/{code}"),
            new HATEOASLink("list", "/api/vatTaxes")
        };

        return Ok(new SingleVatTaxResponseDTO { Item = result, Links = links });
    }
}
