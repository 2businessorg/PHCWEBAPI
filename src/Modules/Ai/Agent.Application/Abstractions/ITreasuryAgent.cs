using Agent.Application.Chat;

namespace Agent.Application.Abstractions;

/// <summary>
/// Agent host: local model ↔ tool-call loop ↔ MCP client.
/// </summary>
public interface ITreasuryAgent
{
    Task<AgentChatResult> ChatAsync(string userMessage, CancellationToken cancellationToken = default);
}

/// <summary>
/// Final agent response, including debug metadata for the POC.
/// </summary>
public record AgentChatResult(
    string Answer,
    string Model,
    int Iterations,
    IReadOnlyList<AgentToolCallLog> ToolCalls);

/// <summary>
/// Debug record of a tool call executed during the agent loop.
/// </summary>
public record AgentToolCallLog(string Name, string Arguments, bool IsError, int DurationMs);
