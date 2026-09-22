using Microsoft.EntityFrameworkCore;
using Treasury.Domain.Entities;
using Treasury.Domain.Repositories;
using Treasury.Infrastructure.Persistence;

namespace Treasury.Infrastructure.Repositories;

/// <summary>
/// EF Core read-only repository for unreconciled BA/BR movements.
/// </summary>
public class BankReconciliationRepositoryEFCore : IBankReconciliationRepository
{
    private readonly TreasuryDbContext _context;

    public BankReconciliationRepositoryEFCore(TreasuryDbContext context)
    {
        _context = context;
    }

    public async Task<(int TotalCount, IReadOnlyList<ImportedBankMovement> Items)> GetImportedBankMovementsAsync(
        decimal accountCode,
        DateTime dateFrom,
        DateTime dateToExclusive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ImportedBankMovements
            .AsNoTracking()
            .Where(m =>
                m.AccountCode == accountCode &&
                !m.Reconciled &&
                !m.Ignored &&
                m.Date >= dateFrom &&
                m.Date < dateToExclusive);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (total, items);
    }

    public async Task<(int TotalCount, IReadOnlyList<TreasuryAccountMovement> Items)> GetTreasuryAccountMovementsAsync(
        decimal accountCode,
        DateTime dateFrom,
        DateTime dateToExclusive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TreasuryAccountMovements
            .AsNoTracking()
            .Where(m =>
                m.AccountCode == accountCode &&
                !m.Reconciled &&
                m.Date >= dateFrom &&
                m.Date < dateToExclusive);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (total, items);
    }
}
