using Microsoft.EntityFrameworkCore;
using Treasury.Domain.Entities;
using Treasury.Domain.Repositories;
using Treasury.Infrastructure.Persistence;

namespace Treasury.Infrastructure.Repositories;

/// <summary>
/// EF Core read-only repository for treasury accounts (PHC <c>bl</c>).
/// </summary>
public class TreasuryAccountRepositoryEFCore : ITreasuryAccountRepository
{
    private readonly TreasuryDbContext _context;

    public TreasuryAccountRepositoryEFCore(TreasuryDbContext context)
    {
        _context = context;
    }

    public async Task<TreasuryAccount?> GetByNameAsync(string accountName, CancellationToken cancellationToken = default)
    {
        var normalized = accountName.Trim();

        return await _context.TreasuryAccounts
            .AsNoTracking()
            .Where(a => a.Name.Trim() == normalized)
            .OrderBy(a => a.AccountCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(int TotalCount, IReadOnlyList<TreasuryAccount> Items)> ListAsync(
        string? nameContains,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TreasuryAccounts.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(a => !a.Inactive);
        }

        if (!string.IsNullOrWhiteSpace(nameContains))
        {
            var fragment = nameContains.Trim();
            query = query.Where(a => a.Name.Contains(fragment));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(a => a.Name)
            .ThenBy(a => a.AccountCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (total, items);
    }
}
