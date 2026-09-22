using Treasury.Domain.Entities;

namespace Treasury.Domain.Repositories;

/// <summary>
/// Read-only persistence for treasury accounts (PHC <c>bl</c>).
/// </summary>
public interface ITreasuryAccountRepository
{
    /// <summary>
    /// Finds an account by display name (<c>banco</c>), trimmed and case-insensitive.
    /// </summary>
    Task<TreasuryAccount?> GetByNameAsync(string accountName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists treasury accounts, optionally filtered by name fragment.
    /// </summary>
    Task<(int TotalCount, IReadOnlyList<TreasuryAccount> Items)> ListAsync(
        string? nameContains,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
