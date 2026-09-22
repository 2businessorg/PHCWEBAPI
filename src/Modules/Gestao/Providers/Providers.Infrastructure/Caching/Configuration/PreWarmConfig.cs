namespace Providers.Infrastructure.Caching.Configuration;

/// <summary>
/// Configuration for pre-warming a specific provider configuration at startup.
/// </summary>
public class PreWarmConfig
{
    /// <summary>
    /// Provider name (e.g., "JWT", "TFR", "MPDC").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Optional operation code. If null, pre-warms all operations for the provider.
    /// </summary>
    public string? OperationCode { get; set; }

    /// <summary>
    /// Environment name (e.g., "Development", "Production").
    /// </summary>
    public string Environment { get; set; } = "Development";
}
