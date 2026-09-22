using System.Collections.Concurrent;
using Shared.Abstractions.Privacy.Pseudonymization;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Store;

/// <summary>In-memory TokenMap for unit tests and local development.</summary>
public sealed class InMemoryTokenMapStore : ITokenMapStore
{
    private readonly ConcurrentDictionary<string, TokenMapEntry> _entries = new(StringComparer.Ordinal);

    public Task UpsertAsync(TokenMapEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _entries[Key(entry.SessionId, entry.DocumentId, entry.Token)] = entry;
        return Task.CompletedTask;
    }

    public Task<TokenMapEntry?> GetByTokenAsync(
        string sessionId,
        string documentId,
        string token,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _entries.TryGetValue(Key(sessionId, documentId, token), out var entry);
        if (entry is not null && entry.ExpiresAt <= DateTimeOffset.UtcNow)
            return Task.FromResult<TokenMapEntry?>(null);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<TokenMapEntry>> ListBySessionDocumentAsync(
        string sessionId,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        IReadOnlyList<TokenMapEntry> list = _entries.Values
            .Where(e => e.SessionId == sessionId
                        && e.DocumentId == documentId
                        && e.ExpiresAt > now)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        var expired = _entries.Where(kv => kv.Value.ExpiresAt <= now).Select(kv => kv.Key).ToList();
        foreach (var key in expired)
            _entries.TryRemove(key, out _);
        return Task.FromResult(expired.Count);
    }

    private static string Key(string sessionId, string documentId, string token) =>
        $"{sessionId}|{documentId}|{token}";
}
