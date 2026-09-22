namespace Agent.Infrastructure.Options;

/// <summary>
/// Local model runtime options. Bound from configuration section <c>LocalAI</c>.
/// </summary>
public sealed class LocalAiOptions
{
    public const string SectionName = "LocalAI";

    public string Provider { get; set; } = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "qwen2.5-coder:1.5b";

    /// <summary>API key for cloud providers (Qwen / DashScope). Leave empty for Ollama.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// HttpClient timeout for chat calls.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 1200;

    /// <summary>
    /// When false, disables Qwen3-style reasoning ("think") for faster tool calls.
    /// </summary>
    public bool Think { get; set; } = false;
}
