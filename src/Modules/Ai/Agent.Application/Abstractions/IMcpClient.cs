using System.Text.Json;
using Agent.Application.Chat;

namespace Agent.Application.Abstractions;

/// <summary>
/// MCP client used by the agent host. The local model never talks to MCP directly.
/// </summary>
public interface IMcpClient
{
    Task<IReadOnlyList<ToolDefinition>> ListToolsAsync(CancellationToken cancellationToken = default);

    Task<McpToolResult> CallToolAsync(
        string name,
        JsonElement arguments,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an MCP tool invocation.
/// </summary>
public record McpToolResult(bool IsError, string Content);
