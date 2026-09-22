using Agent.Application.Abstractions;
using Agent.Application.Features.SendAgentChat;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Kernel.Authorization;
using Shared.Kernel.Responses;

namespace Agent.Presentation.REST.Controllers;

/// <summary>
/// POC chat endpoint for the local treasury agent.
/// </summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public sealed class AiChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Sends a natural-language prompt to the local Qwen agent.
    /// </summary>
    [HttpPost("chat")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("ai-chat")]
    [ProducesResponseType(typeof(AgentChatResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentChatResponseDTO>> Chat(
        [FromBody] AgentChatRequestDTO request,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SendAgentChatCommand(request.Message), ct);

        return Ok(new AgentChatResponseDTO
        {
            Answer = result.Answer,
            Model = result.Model,
            Iterations = result.Iterations,
            ToolCalls = result.ToolCalls.Select(t => new AgentToolCallResponseDTO
            {
                Name = t.Name,
                ArgumentsJson = t.Arguments,
                IsError = t.IsError,
                DurationMs = t.DurationMs
            }).ToList()
        });
    }
}

/// <summary>
/// Chat request payload.
/// </summary>
public sealed class AgentChatRequestDTO
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Chat response payload, including POC tool-call metadata.
/// </summary>
public sealed class AgentChatResponseDTO
{
    public string Answer { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Iterations { get; set; }

    public List<AgentToolCallResponseDTO> ToolCalls { get; set; } = [];
}

/// <summary>
/// Debug metadata for one MCP tool call.
/// </summary>
public sealed class AgentToolCallResponseDTO
{
    public string Name { get; set; } = string.Empty;

    public string ArgumentsJson { get; set; } = string.Empty;

    public bool IsError { get; set; }

    public int DurationMs { get; set; }
}
