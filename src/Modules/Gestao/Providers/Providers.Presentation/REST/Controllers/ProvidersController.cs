using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Providers.Application.Features.CreateProvider;
using Providers.Application.Features.UpdateProvider;
using Providers.Application.Features.DeleteProvider;
using Providers.Application.Features.GetAllProviders;
using Providers.Application.Features.GetProviderByStamp;
using Providers.Application.Features.GetProviderConfig;
using Shared.Kernel.Responses;
using Shared.Kernel.Extensions;
using Shared.Kernel.Authorization;
using Providers.Application.DTOs.Providers;

namespace Providers.Presentation.REST.Controllers;

[ApiController]
[Route("api/providers")]
[Produces("application/json")]
public sealed class ProvidersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProvidersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtém todos os providers
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetAll(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? environment = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllProvidersQuery(includeInactive, environment), ct);
        var correlationId = HttpContext.GetCorrelationId();
        return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    /// <summary>
    /// Obtém provider por stamp
    /// </summary>
    [HttpGet("{uProviderStamp}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetByStamp(
        string uProviderStamp,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProviderByStampQuery(uProviderStamp), ct);
        var correlationId = HttpContext.GetCorrelationId();

        return result is null
            ? NotFound(ResponseDTO.Error(ResponseCodes.Provider.NotFound, correlationId: correlationId))
            : Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    /// <summary>
    /// ⭐ ENDPOINT PRINCIPAL - Obtém configuração completa de um endpoint
    /// </summary>
    [HttpGet("config/{provedor}/{operationCode}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetConfig(
        string provedor,
        string operationCode,
        [FromQuery] string environment = "Development",
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProviderConfigQuery(provedor, operationCode, environment), ct);
        var correlationId = HttpContext.GetCorrelationId();

        return result is null
            ? NotFound(ResponseDTO.Error(ResponseCodes.Provider.ConfigNotFound, correlationId: correlationId))
            : Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    /// <summary>
    /// Cria um novo provider
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-create")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Create(
        [FromBody] CreateProviderInputDTO dto,
        CancellationToken ct = default)
    {
        var command = new CreateProviderCommand(dto, User.Identity?.Name);
        var result = await _mediator.Send(command, ct);
        var correlationId = HttpContext.GetCorrelationId();

        return CreatedAtAction(
            nameof(GetByStamp),
            new { uProviderStamp = result.UProviderStamp },
            ResponseDTO.Success(data: result, content: "Provider created successfully", correlationId: correlationId));
    }

    /// <summary>
    /// Atualiza um provider
    /// </summary>
    [HttpPut("{uProviderStamp}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-update")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Update(
        string uProviderStamp,
        [FromBody] UpdateProviderInputDTO dto,
        CancellationToken ct = default)
    {
        var command = new UpdateProviderCommand(uProviderStamp, dto, User.Identity?.Name);
        var result = await _mediator.Send(command, ct);
        var correlationId = HttpContext.GetCorrelationId();
        return Ok(ResponseDTO.Success(data: result, content: "Provider updated successfully", correlationId: correlationId));
    }

    /// <summary>
    /// Deleta um provider
    /// </summary>
    [HttpDelete("{uProviderStamp}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("parameters-delete")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Delete(
        string uProviderStamp,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteProviderCommand(uProviderStamp), ct);
        var correlationId = HttpContext.GetCorrelationId();

        return result
            ? Ok(ResponseDTO.Success(content: ResponseCodes.Provider.DeletedSuccessfully, correlationId: correlationId))
            : NotFound(ResponseDTO.Error(ResponseCodes.Provider.NotFound, correlationId: correlationId));
    }
}
