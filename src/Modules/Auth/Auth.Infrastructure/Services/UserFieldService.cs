using Auth.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shared.Kernel.UserFields;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Implementation of IUserFieldService with in-memory caching.
/// Cache key: "{appLicenseStamp}:{moduleName}:{tableName}"
/// Cache duration: 5 minutes (user fields change infrequently).
/// </summary>
public class UserFieldService : IUserFieldService
{
    private readonly IUserFieldRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserFieldService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public UserFieldService(
        IUserFieldRepository repository,
        IMemoryCache cache,
        ILogger<UserFieldService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserFieldDefinition>> GetFieldsAsync(
        string appLicenseStamp,
        string moduleName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"userfields:{appLicenseStamp.Trim()}:{moduleName.Trim()}:{tableName.Trim()}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<UserFieldDefinition>? cached) && cached is not null)
        {
            _logger.LogDebug("UserField cache hit: {CacheKey}", cacheKey);
            return cached;
        }

        var entities = await _repository.GetActiveFieldsAsync(
            appLicenseStamp, moduleName, tableName, cancellationToken);

        var definitions = entities
            .Select(f => new UserFieldDefinition(
                Alias: f.FieldAlias.Trim(),
                ColumnName: f.FieldColumn.Trim(),
                FieldType: f.FieldType.Trim(),
                IsReadOnly: f.IsReadOnly))
            .ToList()
            .AsReadOnly();

        _cache.Set(cacheKey, (IReadOnlyList<UserFieldDefinition>)definitions, CacheDuration);

        _logger.LogDebug(
            "UserField cache miss - loaded {Count} field(s) for {CacheKey}",
            definitions.Count, cacheKey);

        return definitions;
    }
}
