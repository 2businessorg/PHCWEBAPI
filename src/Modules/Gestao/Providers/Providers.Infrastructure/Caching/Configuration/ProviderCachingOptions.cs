namespace Providers.Infrastructure.Caching.Configuration;

/// <summary>
/// Configuration options for provider caching system.
/// Reads from "Caching:Providers" section in appsettings.json.
/// </summary>
public class ProviderCachingOptions
{
    /// <summary>
    /// Whether caching is enabled globally.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum cache size in megabytes.
    /// </summary>
    public int MaxCacheSizeMB { get; set; } = 100;

    /// <summary>
    /// Whether to pre-warm cache at startup.
    /// </summary>
    public bool EnablePreWarming { get; set; } = true;

    /// <summary>
    /// Default policy applied when no specific policy exists for a provider.
    /// </summary>
    public ProviderPolicySettings DefaultPolicy { get; set; } = new();

    /// <summary>
    /// Provider-specific caching policies.
    /// Key: Provider name (e.g., "JWT", "TFR", "MPDC")
    /// Value: Policy settings for that provider
    /// </summary>
    public Dictionary<string, ProviderPolicySettings> ProviderPolicies { get; set; } = new();

    /// <summary>
    /// List of providers to pre-warm at startup.
    /// </summary>
    public List<PreWarmConfig> PreWarmProviders { get; set; } = new();

    /// <summary>
    /// Monitoring and logging settings.
    /// </summary>
    public MonitoringSettings Monitoring { get; set; } = new();
}
