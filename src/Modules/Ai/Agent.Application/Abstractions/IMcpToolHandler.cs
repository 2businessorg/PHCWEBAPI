using System.Text.Json;

namespace Agent.Application.Abstractions;

/// <summary>
/// In-process MCP tool handler. The HTTP MCP server and the in-process MCP client
/// both execute the same handlers so REST and MCP share Treasury.Application.
/// </summary>
public interface IMcpToolHandler
{
    string Name { get; }

    string Description { get; }

    JsonElement InputSchema { get; }

    Task<string> InvokeAsync(JsonElement arguments, CancellationToken cancellationToken);
}
