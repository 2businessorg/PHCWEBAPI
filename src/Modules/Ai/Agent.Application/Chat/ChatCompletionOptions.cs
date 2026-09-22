namespace Agent.Application.Chat;

/// <summary>
/// Per-call options for the chat model (JSON mode, etc.).
/// </summary>
public sealed record ChatCompletionOptions(bool JsonObjectResponse = false)
{
    public static ChatCompletionOptions JsonObject { get; } = new(true);
}
