using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Parameters.Application.Features.CreateParameter;
using Parameters.Application.Features.UpdateParameter;
using Parameters.Application.Features.DeleteParameter;
using Parameters.Application.Features.GetAllParameters;
using Parameters.Application.Features.GetParameterByStamp;
using Shared.Kernel.Responses;
using Shared.Kernel.Extensions;
using Shared.Kernel.Authorization;
using Parameters.Application.DTOs.Parameters;

namespace Parameters.Presentation.REST.Controllers;


[ApiController]
[Route("api/parameters")]
[Produces("application/json")]
public sealed class ParametersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ParametersController(IMediator mediator) => _mediator = mediator;


    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllParametersQuery(includeInactive), ct);
        var correlationId = HttpContext.GetCorrelationId();
        return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    [HttpGet("{para1Stamp}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-query")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetByStamp(
        string para1Stamp,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetParameterByStampQuery(para1Stamp), ct);
        var correlationId = HttpContext.GetCorrelationId();

        return result is null
            ? NotFound(ResponseDTO.Error(ResponseCodes.Parameter.NotFound, correlationId: correlationId))
            : Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-create")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Create(
        [FromBody] CreateParameterInputDTO dto,
        CancellationToken ct = default)
    {
        var command = new CreateParameterCommand(dto, User.Identity?.Name);
        var result = await _mediator.Send(command, ct);
        var correlationId = HttpContext.GetCorrelationId();

        return CreatedAtAction(
            nameof(GetByStamp),
            new { para1Stamp = result.Para1Stamp },
            ResponseDTO.Success(data: result, content: ResponseCodes.Parameter.CreatedSuccessfully, correlationId: correlationId));
    }

   
    [HttpPut("{para1Stamp}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("parameters-update")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Update(
        string para1Stamp,
        [FromBody] UpdateParameterInputDTO dto,
        CancellationToken ct = default)
    {
        var command = new UpdateParameterCommand(para1Stamp, dto, User.Identity?.Name);
        var result = await _mediator.Send(command, ct);
        var correlationId = HttpContext.GetCorrelationId();
        return Ok(ResponseDTO.Success(data: result, content: ResponseCodes.Parameter.UpdatedSuccessfully, correlationId: correlationId));
    }


    [HttpDelete("{para1Stamp}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("parameters-delete")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Delete(
        string para1Stamp,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteParameterCommand(para1Stamp), ct);
        var correlationId = HttpContext.GetCorrelationId();

        return result
            ? Ok(ResponseDTO.Success(content: ResponseCodes.Parameter.DeletedSuccessfully, correlationId: correlationId))
            : NotFound(ResponseDTO.Error(ResponseCodes.Parameter.NotFound, correlationId: correlationId));
    }
}
