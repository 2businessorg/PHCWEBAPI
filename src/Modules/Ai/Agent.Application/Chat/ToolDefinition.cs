using System.Text.Json;

namespace Agent.Application.Chat;

/// <summary>
/// JSON schema of a tool presented to the local model.
/// </summary>
public record ToolDefinition(string Name, string Description, JsonElement InputSchema);
