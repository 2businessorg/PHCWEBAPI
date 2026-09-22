namespace Providers.Infrastructure.Caching.Configuration;

/// <summary>
/// Defines caching policy settings for a specific provider.
/// </summary>
public class ProviderPolicySettings
{
    /// <summary>
    /// Time-to-live for cached items in minutes.
    /// </summary>
    public int TTLMinutes { get; set; } = 5;

    /// <summary>
    /// Cache priority: High, Normal, Low, NeverRemove.
    /// </summary>
    public string Priority { get; set; } = "Normal";

    /// <summary>
    /// Whether to cache all operations for this provider.
    /// </summary>
    public bool CacheAll { get; set; } = true;

    /// <summary>
    /// List of operation codes to exclude from caching (e.g., ["AUTH"]).
    /// Only used when CacheAll is false.
    /// </summary>
    public List<string> ExcludeOperations { get; set; } = new();

    /// <summary>
    /// Human-readable description of this policy.
    /// </summary>
    public string? Description { get; set; }
}
