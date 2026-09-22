using System.Text.Json;

namespace Agent.Application.Chat;

/// <summary>
/// Tool call requested by the local model.
/// </summary>
public record ToolCall(string Id, string Name, JsonElement Arguments);
