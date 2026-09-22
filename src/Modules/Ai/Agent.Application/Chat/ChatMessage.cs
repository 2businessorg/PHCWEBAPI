namespace Agent.Application.Chat;

/// <summary>
/// Chat message exchanged with the local model.
/// </summary>
public record ChatMessage(string Role, string Content, IReadOnlyList<ToolCall>? ToolCalls = null, string? ToolCallId = null);
