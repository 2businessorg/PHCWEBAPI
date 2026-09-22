using System.Text.Json;
using Agent.Application.Abstractions;
using Agent.Application.Chat;
using Microsoft.Extensions.Logging;

namespace Agent.Infrastructure.Mcp;

/// <summary>
/// In-process MCP client. Lists and executes the same tool handlers exposed by the MCP HTTP server.
/// </summary>
public sealed class InProcessMcpClient : IMcpClient
{
    private readonly IEnumerable<IMcpToolHandler> _handlers;
    private readonly ILogger<InProcessMcpClient> _logger;

    public InProcessMcpClient(
        IEnumerable<IMcpToolHandler> handlers,
        ILogger<InProcessMcpClient> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    public Task<IReadOnlyList<ToolDefinition>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ToolDefinition> tools = _handlers
            .Select(h => new ToolDefinition(h.Name, h.Description, h.InputSchema))
            .ToList();

        return Task.FromResult(tools);
    }

    public async Task<McpToolResult> CallToolAsync(
        string name,
        JsonElement arguments,
        CancellationToken cancellationToken = default)
    {
        var handler = _handlers.FirstOrDefault(h =>
            string.Equals(h.Name, name, StringComparison.Ordinal));

        if (handler is null)
        {
            _logger.LogWarning("MCP tool not found. Name={Tool}", name);
            return new McpToolResult(true, $"Unknown tool '{name}'.");
        }

        try
        {
            var content = await handler.InvokeAsync(arguments, cancellationToken);
            return new McpToolResult(false, content);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "MCP tool failed. Name={Tool}", name);
            return new McpToolResult(true, ex.Message);
        }
    }
}
