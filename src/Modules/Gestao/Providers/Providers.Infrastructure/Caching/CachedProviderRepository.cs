using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Providers.Domain.Entities;
using Providers.Domain.Repositories;
using Providers.Infrastructure.Caching.Configuration;

namespace Providers.Infrastructure.Caching;

/// <summary>
/// Decorator Pattern implementation that adds caching to IProviderRepository.
/// Wraps the actual repository and caches frequently accessed provider configurations.
/// </summary>
public class CachedProviderRepository : IProviderRepository
{
    private readonly IProviderRepository _innerRepository;
    private readonly IMemoryCache _cache;
    private readonly IProviderCachePolicy _cachePolicy;
    private readonly ILogger<CachedProviderRepository> _logger;
    private readonly ProviderCachingOptions _options;

    public CachedProviderRepository(
        IProviderRepository innerRepository,
        IMemoryCache cache,
        IProviderCachePolicy cachePolicy,
        ILogger<CachedProviderRepository> logger,
        IOptions<ProviderCachingOptions> options)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _cachePolicy = cachePolicy;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Provider?> GetByStampAsync(string uProviderStamp, CancellationToken cancellationToken = default)
    {
        // Simple pass-through - stamps are unique identifiers, not worth caching by stamp
        return await _innerRepository.GetByStampAsync(uProviderStamp, cancellationToken);
    }

    public async Task<Provider?> GetByCodigoAsync(int codigo, CancellationToken cancellationToken = default)
    {
        // Simple pass-through - codigo lookups are less frequent
        return await _innerRepository.GetByCodigoAsync(codigo, cancellationToken);
    }

    public async Task<Provider?> GetByProviderAndEnvironmentAsync(
        string provedor, 
        string environment, 
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !_cachePolicy.ShouldCache(provedor))
        {
            return await _innerRepository.GetByProviderAndEnvironmentAsync(provedor, environment, cancellationToken);
        }

        var cacheKey = _cachePolicy.GetCacheKey(provedor, null, environment);

        // Try to get from cache
        if (_cache.TryGetValue<Provider>(cacheKey, out var cachedProvider))
        {
            if (_options.Monitoring.LogCacheHits)
            {
                _logger.LogDebug("Cache HIT: {CacheKey} - Provider: {Provider}, Environment: {Environment}", 
                    cacheKey, provedor, environment);
            }
            return cachedProvider;
        }

        // Cache miss - fetch from database
        if (_options.Monitoring.LogCacheMisses)
        {
            _logger.LogDebug("Cache MISS: {CacheKey} - Provider: {Provider}, Environment: {Environment}", 
                cacheKey, provedor, environment);
        }

        var provider = await _innerRepository.GetByProviderAndEnvironmentAsync(provedor, environment, cancellationToken);

        if (provider != null)
        {
            // Store in cache with policy-based TTL and priority
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_cachePolicy.GetTTL(provedor))
                .SetPriority(_cachePolicy.GetPriority(provedor))
                .SetSize(1); // Each entry counts as 1 unit towards size limit

            if (_options.Monitoring.LogEvictions)
            {
                cacheEntryOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _logger.LogInformation("Cache EVICTION: {CacheKey} - Reason: {Reason}", key, reason);
                });
            }

            _cache.Set(cacheKey, provider, cacheEntryOptions);

            _logger.LogDebug("Cached Provider: {Provider}, Environment: {Environment}, TTL: {TTL}",
                provedor, environment, _cachePolicy.GetTTL(provedor));
        }

        return provider;
    }

    public async Task<IEnumerable<Provider>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        // Pass-through for list operations - caching large lists can be memory-intensive
        return await _innerRepository.GetAllAsync(includeInactive, cancellationToken);
    }

    public async Task<IEnumerable<Provider>> GetByEnvironmentAsync(
        string environment, 
        bool includeInactive = false, 
        CancellationToken cancellationToken = default)
    {
        // Pass-through for list operations
        return await _innerRepository.GetByEnvironmentAsync(environment, includeInactive, cancellationToken);
    }

    public async Task<IEnumerable<Provider>> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        // Pass-through for paged operations
        return await _innerRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string uProviderStamp, CancellationToken cancellationToken = default)
    {
        // Pass-through for existence checks
        return await _innerRepository.ExistsAsync(uProviderStamp, cancellationToken);
    }

    public async Task<bool> ExistsProviderEnvironmentAsync(
        string provedor, 
        string environment, 
        CancellationToken cancellationToken = default)
    {
        // Pass-through for existence checks
        return await _innerRepository.ExistsProviderEnvironmentAsync(provedor, environment, cancellationToken);
    }

    public async Task<Provider> AddAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        var result = await _innerRepository.AddAsync(provider, cancellationToken);
        
        // Invalidate cache for this provider to ensure consistency
        InvalidateProviderCache(provider.Provedor, provider.Environment);
        
        return result;
    }

    public async Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(provider, cancellationToken);
        
        // Invalidate cache for this provider to ensure consistency
        InvalidateProviderCache(provider.Provedor, provider.Environment);
    }

    public async Task DeleteAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        await _innerRepository.DeleteAsync(provider, cancellationToken);
        
        // Invalidate cache for this provider to ensure consistency
        InvalidateProviderCache(provider.Provedor, provider.Environment);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        // Pass-through for count operations
        return await _innerRepository.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Invalidates cache entries for a specific provider and environment.
    /// </summary>
    private void InvalidateProviderCache(string provedor, string environment)
    {
        var cacheKey = _cachePolicy.GetCacheKey(provedor, null, environment);
        _cache.Remove(cacheKey);
        
        _logger.LogDebug("Cache INVALIDATED: {CacheKey} - Provider: {Provider}, Environment: {Environment}",
            cacheKey, provedor, environment);
    }
}
