namespace Providers.Infrastructure.Caching.Configuration;

/// <summary>
/// Monitoring configuration for provider caching.
/// </summary>
public class MonitoringSettings
{
    /// <summary>
    /// Whether to log cache hits.
    /// </summary>
    public bool LogCacheHits { get; set; } = true;

    /// <summary>
    /// Whether to log cache misses.
    /// </summary>
    public bool LogCacheMisses { get; set; } = true;

    /// <summary>
    /// Whether to log cache evictions.
    /// </summary>
    public bool LogEvictions { get; set; } = false;
}
