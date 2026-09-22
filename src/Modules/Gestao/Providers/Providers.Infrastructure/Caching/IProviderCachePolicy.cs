using Microsoft.Extensions.Caching.Memory;

namespace Providers.Infrastructure.Caching;

/// <summary>
/// Defines caching policy behavior for provider configurations.
/// </summary>
public interface IProviderCachePolicy
{
    /// <summary>
    /// Gets the time-to-live for cached items of a specific provider.
    /// </summary>
    /// <param name="providerName">Provider name (e.g., "JWT", "TFR", "MPDC")</param>
    /// <returns>TimeSpan representing the cache TTL</returns>
    TimeSpan GetTTL(string providerName);

    /// <summary>
    /// Gets the cache priority for items of a specific provider.
    /// </summary>
    /// <param name="providerName">Provider name</param>
    /// <returns>CacheItemPriority enum value</returns>
    CacheItemPriority GetPriority(string providerName);

    /// <summary>
    /// Determines whether a specific provider operation should be cached.
    /// </summary>
    /// <param name="providerName">Provider name</param>
    /// <param name="operationCode">Operation code (optional)</param>
    /// <returns>True if should be cached, false otherwise</returns>
    bool ShouldCache(string providerName, string? operationCode = null);

    /// <summary>
    /// Generates a standardized cache key for a provider configuration.
    /// </summary>
    /// <param name="providerName">Provider name</param>
    /// <param name="operationCode">Operation code (optional)</param>
    /// <param name="environment">Environment name (optional)</param>
    /// <returns>Cache key string</returns>
    string GetCacheKey(string providerName, string? operationCode = null, string? environment = null);
}
