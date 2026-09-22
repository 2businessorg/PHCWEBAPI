namespace Agent.Application.Chat;

/// <summary>
/// Response from the local chat model for one turn.
/// </summary>
public record ChatModelResponse(string? Content, IReadOnlyList<ToolCall> ToolCalls)
{
    public bool HasToolCalls => ToolCalls.Count > 0;
}
