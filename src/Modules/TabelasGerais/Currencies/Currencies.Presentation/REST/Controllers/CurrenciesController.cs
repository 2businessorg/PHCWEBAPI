using MediatR;
using FluentValidation;
using Currencies.Application.Errors;
using Currencies.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Currencies.Application.Features.GetAllCurrencies;
using Currencies.Application.Features.GetCurrencyByCode;
using Currencies.Application.Features.GetAllCurrencyExchangeRates;
using Currencies.Application.Features.GetCurrencyExchangeRates;
using Currencies.Application.Features.CreateCurrencyExchangeRate;
using Currencies.Application.Features.CreateCurrency;
using Currencies.Application.Features.UpdateCurrency;
using Currencies.Application.Features.DeleteCurrency;
using Shared.Kernel.Authorization;
using Shared.Kernel.Responses;

namespace Currencies.Presentation.REST.Controllers;

/// <summary>
/// Controller para gerenciar moedas.
/// </summary>
[ApiController]
[Route("api/currencies")]
[Produces("application/json")]
public sealed class CurrenciesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Inicializa uma nova instância do controller.
    /// </summary>
    public CurrenciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista todas as moedas disponíveis.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAll(CancellationToken ct = default)
    {
        var items = (await _mediator.Send(new GetAllCurrenciesQuery(), ct)).ToList();
        var links = new List<HATEOASLink>
        {
            new("self", "/api/currencies", "GET")
        };

        return Ok(CollectionResponseDTO.FromPaged(items, items.Count, 1, items.Count == 0 ? 1 : items.Count, links));
    }

    /// <summary>
    /// Obtém uma moeda específica pelo código.
    /// </summary>
    [HttpGet("{moeda}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(SingleResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SingleResponseDTO>> GetByCode(string moeda, CancellationToken ct = default)
    {
        var query = new GetCurrencyByCodeQuery(moeda);
        var item = await _mediator.Send(query, ct);

        if (item is null)
            return NotFound(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));

        var links = new List<HATEOASLink>
        {
            new("self", $"/api/currencies/{moeda}", "GET"),
            new("list", "/api/currencies", "GET")
        };

        return Ok(SingleResponseDTO.FromItem(item, links));
    }

    /// <summary>
    /// Obtém todas as taxas de conversão.
    /// </summary>
    [HttpGet("exchangeRates")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAllExchangeRates(CancellationToken ct = default)
    {
        var items = (await _mediator.Send(new GetAllCurrencyExchangeRatesQuery(), ct)).ToList();

        var links = new List<HATEOASLink>
        {
            new("self", "/api/currencies/exchangeRates", "GET"),
            new("currencies", "/api/currencies", "GET")
        };

        return Ok(CollectionResponseDTO.FromPaged(items, items.Count, 1, items.Count == 0 ? 1 : items.Count, links));
    }

    /// <summary>
    /// Obtém taxas de conversão de uma moeda.
    /// </summary>
    [HttpGet("exchangeRates/{moeda}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetExchangeRates(string moeda, CancellationToken ct = default)
    {
        var items = (await _mediator.Send(new GetCurrencyExchangeRatesQuery(moeda), ct)).ToList();

        if (items.Count == 0)
            return NotFound(CollectionResponseDTO.FromPaged(Enumerable.Empty<object>(), 0, 1, 1));

        var links = new List<HATEOASLink>
        {
            new("self", $"/api/currencies/exchangeRates/{moeda}", "GET"),
            new("currency", $"/api/currencies/{moeda}", "GET"),
            new("list", "/api/currencies", "GET")
        };

        return Ok(CollectionResponseDTO.FromPaged(items, items.Count, 1, items.Count == 0 ? 1 : items.Count, links));
    }

    /// <summary>
    /// Cria uma nova taxa de conversão.
    /// </summary>
    [HttpPost("exchangeRates")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-create")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> CreateExchangeRate([FromBody] CreateCurrencyExchangeRateInputDTO dto, CancellationToken ct = default)
    {
        if (dto is null)
            return BadRequest(PaginatedResponseDTO.Error(CurrenciesErrorCatalog.ValidationError.Code, CurrenciesErrorCatalog.ValidationError.Description));

        try
        {
            var createdBy = User.Identity?.Name ?? "PHCAPI";

            var item = await _mediator.Send(new CreateCurrencyExchangeRateCommand(dto, createdBy), ct);

            var links = new List<HATEOASLink>
            {
                new("self", $"/api/currencies/exchangeRates/{item.Moeda}", "GET"),
                new("rates", "/api/currencies/exchangeRates", "GET"),
                new("currency", $"/api/currencies/{item.Moeda}", "GET")
            };

            var response = new PaginatedResponseDTO(
                CurrenciesErrorCatalog.Success.Code,
                "Taxa de conversão criada com sucesso",
                new[] { (object)item },
                links);

            return CreatedAtAction(nameof(GetExchangeRates), new { moeda = item.Moeda }, response);
        }
        catch (CurrenciesModuleException ex)
        {
            var links = new List<HATEOASLink>();

            // Adiciona links apropriados baseado no tipo de erro
            if (ex.Code == CurrenciesErrorCatalog.InvalidExchangeRateCombination.Code ||
                ex.Code == CurrenciesErrorCatalog.CurrencyNotFound.Code)
            {
                links.Add(new("get", "/api/currencies", "GET"));
                links.Add(new("create", "/api/currencies", "POST"));
            }

            return BadRequest(new PaginatedResponseDTO(ex.Code, ex.Message, null, links.Count > 0 ? links : null));
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var msg = first?.ErrorMessage ?? ex.Message;
            var code = !string.IsNullOrWhiteSpace(first?.ErrorCode) && first!.ErrorCode.StartsWith("CUR")
                ? first.ErrorCode
                : CurrenciesErrorCatalog.ValidationError.Code;
            return BadRequest(PaginatedResponseDTO.Error(code, msg));
        }
    }

    /// <summary>
    /// Atualiza uma moeda existente.
    /// </summary>
    [HttpPatch("{moeda}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-update")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Update(string moeda, [FromBody] UpdateCurrencyRequest request, CancellationToken ct = default)
    {
        if (request is null)
            return BadRequest(PaginatedResponseDTO.Error(CurrenciesErrorCatalog.ValidationError.Code, CurrenciesErrorCatalog.ValidationError.Description));

        try
        {
            var updatedBy = User.Identity?.Name ?? "PHCAPI";
            var item = await _mediator.Send(new UpdateCurrencyCommand(moeda, request.Pais, updatedBy), ct);

            if (item is null)
            {
                return NotFound(PaginatedResponseDTO.Error(
                    CurrenciesErrorCatalog.CurrencyNotFound.Code,
                    CurrenciesErrorCatalog.CurrencyNotFound.Description));
            }

            var links = new List<HATEOASLink>
            {
                new("self", $"/api/currencies/{item.Moeda}", "GET"),
                new("list", "/api/currencies", "GET")
            };

            return Ok(new PaginatedResponseDTO(
                CurrenciesErrorCatalog.Success.Code,
                "Moeda atualizada com sucesso",
                new[] { (object)item },
                links));
        }
        catch (CurrenciesModuleException ex)
        {
            return BadRequest(new PaginatedResponseDTO(ex.Code, ex.Message, null, null));
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var msg = first?.ErrorMessage ?? ex.Message;
            var code = !string.IsNullOrWhiteSpace(first?.ErrorCode) && first!.ErrorCode.StartsWith("CUR")
                ? first.ErrorCode
                : CurrenciesErrorCatalog.ValidationError.Code;
            return BadRequest(PaginatedResponseDTO.Error(code, msg));
        }
    }

    /// <summary>
    /// Cria uma nova moeda.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-create")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Create([FromBody] CreateCurrencyInputDTO dto, CancellationToken ct = default)
    {
        if (dto is null)
            return BadRequest(PaginatedResponseDTO.Error(CurrenciesErrorCatalog.ValidationError.Code, CurrenciesErrorCatalog.ValidationError.Description));

        try
        {
            var createdBy = User.Identity?.Name ?? "PHCAPI";
            var command = new CreateCurrencyCommand(dto, createdBy);
            var item = await _mediator.Send(command, ct);

            var links = new List<HATEOASLink>
            {
                new("self", $"/api/currencies/{item.Moeda}", "GET"),
                new("list", "/api/currencies", "GET")
            };

            var response = new PaginatedResponseDTO(
                CurrenciesErrorCatalog.Success.Code,
                "Moeda criada com sucesso",
                new[] { (object)item },
                links);

            return CreatedAtAction(nameof(GetByCode), new { moeda = item.Moeda }, response);
        }
        catch (CurrenciesModuleException ex)
        {
            return BadRequest(new PaginatedResponseDTO(ex.Code, ex.Message, null, null));
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var msg = first?.ErrorMessage ?? ex.Message;
            var code = !string.IsNullOrWhiteSpace(first?.ErrorCode) && first!.ErrorCode.StartsWith("CUR")
                ? first.ErrorCode
                : CurrenciesErrorCatalog.ValidationError.Code;
            return BadRequest(PaginatedResponseDTO.Error(code, msg));
        }
    }

    /// <summary>
    /// Deleta uma moeda existente.
    /// </summary>
    [HttpDelete("{moeda}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-delete")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Delete(string moeda, CancellationToken ct = default)
    {
        var deleted = await _mediator.Send(new DeleteCurrencyCommand(moeda), ct);

        if (!deleted)
        {
            return NotFound(PaginatedResponseDTO.Error(
                CurrenciesErrorCatalog.CurrencyNotFound.Code,
                CurrenciesErrorCatalog.CurrencyNotFound.Description));
        }

        var links = new List<HATEOASLink>
        {
            new("list", "/api/currencies", "GET")
        };

        return Ok(new PaginatedResponseDTO(
            CurrenciesErrorCatalog.Success.Code,
            "Moeda eliminada com sucesso",
            null,
            links));
    }
}
