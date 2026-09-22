using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Audit.Application.Features.GetAllAuditLogs;
using Audit.Application.Features.GetAuditLogByStamp;
using Audit.Application.Features.GetAuditLogsByCorrelationId;
using Shared.Kernel.Responses;
using Shared.Kernel.Extensions;
using Shared.Kernel.Authorization;

namespace Audit.Presentation.REST.Controllers;

/// <summary>
/// Controller para consulta de logs de auditoria
/// ⚠️ ACESSO RESTRITO: Apenas usuários internos (Administrator, InternalUser, AuditViewer)
/// </summary>
[ApiController]
[Route("api/audit")]
[Produces("application/json")]
[Authorize(Policy = AppPolicies.InternalOnly)]
public sealed class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Busca todos os logs de auditoria com filtros opcionais
    /// </summary>
    /// <param name="startDate">Data inicial (opcional)</param>
    /// <param name="endDate">Data final (opcional)</param>
    /// <param name="correlationId">Correlation ID (opcional)</param>
    /// <param name="operation">Nome da operação (opcional)</param>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lista paginada de logs de auditoria com metadata</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetAll(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? correlationId = null,
        [FromQuery] string? operation = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = new GetAllAuditLogsQuery(startDate, endDate, correlationId, operation, pageNumber, pageSize);
        var result = await _mediator.Send(query, ct);
        var currentCorrelationId = HttpContext.GetCorrelationId();

        // ✅ Retornar com metadata de paginação
        return Ok(ResponseDTO.Success(
            data: result,
            content: $"Retrieved {result.Items.Count()} logs out of {result.TotalCount} (Page {result.PageNumber}/{result.TotalPages})",
            correlationId: currentCorrelationId));
    }

    /// <summary>
    /// Busca um log de auditoria específico pelo stamp
    /// </summary>
    /// <param name="uLogsstamp">Identificador único do log</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Log de auditoria encontrado</returns>
    [HttpGet("{uLogsstamp}")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetByStamp(
        string uLogsstamp,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAuditLogByStampQuery(uLogsstamp), ct);
        var correlationId = HttpContext.GetCorrelationId();

        return result is null
            ? NotFound(ResponseDTO.Error(ResponseCodes.NotFound, correlationId: correlationId))
            : Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    /// <summary>
    /// Busca todos os logs relacionados a um Correlation ID específico
    /// Útil para rastrear toda a jornada de uma requisição
    /// </summary>
    /// <param name="correlationId">Correlation ID da requisição</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lista de logs relacionados ao Correlation ID</returns>
    [HttpGet("correlation/{correlationId}")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetByCorrelationId(
        string correlationId,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAuditLogsByCorrelationIdQuery(correlationId), ct);
        var currentCorrelationId = HttpContext.GetCorrelationId();

        return Ok(ResponseDTO.Success(
            data: result,
            content: $"Found {((IEnumerable<dynamic>)result).Count()} logs for correlation ID: {correlationId}",
            correlationId: currentCorrelationId));
    }
}
