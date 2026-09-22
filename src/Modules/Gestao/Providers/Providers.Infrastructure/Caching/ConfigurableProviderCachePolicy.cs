using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Providers.Infrastructure.Caching.Configuration;

namespace Providers.Infrastructure.Caching;

/// <summary>
/// Configurable implementation of IProviderCachePolicy that reads policies from appsettings.json.
/// Uses Strategy Pattern to apply different caching strategies per provider.
/// </summary>
public class ConfigurableProviderCachePolicy : IProviderCachePolicy
{
    private readonly ProviderCachingOptions _options;

    public ConfigurableProviderCachePolicy(IOptions<ProviderCachingOptions> options)
    {
        _options = options.Value;
    }

    public TimeSpan GetTTL(string providerName)
    {
        var policy = GetPolicyForProvider(providerName);
        return TimeSpan.FromMinutes(policy.TTLMinutes);
    }

    public CacheItemPriority GetPriority(string providerName)
    {
        var policy = GetPolicyForProvider(providerName);
        
        return policy.Priority?.ToUpperInvariant() switch
        {
            "HIGH" => CacheItemPriority.High,
            "LOW" => CacheItemPriority.Low,
            "NEVERREMOVE" => CacheItemPriority.NeverRemove,
            _ => CacheItemPriority.Normal
        };
    }

    public bool ShouldCache(string providerName, string? operationCode = null)
    {
        if (!_options.Enabled)
            return false;

        var policy = GetPolicyForProvider(providerName);

        // If CacheAll is true, cache everything
        if (policy.CacheAll)
            return true;

        // If operation code is provided and it's in the exclude list, don't cache
        if (!string.IsNullOrWhiteSpace(operationCode) && 
            policy.ExcludeOperations.Contains(operationCode, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public string GetCacheKey(string providerName, string? operationCode = null, string? environment = null)
    {
        var keyParts = new List<string> { "Provider", providerName };

        if (!string.IsNullOrWhiteSpace(operationCode))
            keyParts.Add(operationCode);

        if (!string.IsNullOrWhiteSpace(environment))
            keyParts.Add(environment);

        return string.Join(":", keyParts);
    }

    /// <summary>
    /// Gets the policy for a specific provider, falling back to default policy if not found.
    /// </summary>
    private ProviderPolicySettings GetPolicyForProvider(string providerName)
    {
        if (_options.ProviderPolicies.TryGetValue(providerName, out var policy))
            return policy;

        return _options.DefaultPolicy;
    }
}
