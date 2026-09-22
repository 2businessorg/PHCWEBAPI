using Advances.Domain.Entities;
using Advances.Domain.Repositories;
using Advances.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Advances.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de Séries de Adiantamento (tsrd)
/// </summary>
public class AdvanceTypeRepositoryEFCore : IAdvanceTypeRepository
{
    private readonly AdvancesDbContext _db;

    public AdvanceTypeRepositoryEFCore(AdvancesDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<Tsrd?> GetByNdocAsync(decimal ndoc, CancellationToken ct = default)
    {
        return await _db.Tsrd
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Ndoc == ndoc, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Tsrd>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Tsrd
            .AsNoTracking()
            .OrderBy(t => t.Ndoc)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNdocAsync(decimal ndoc, CancellationToken ct = default)
    {
        return await _db.Tsrd
            .AnyAsync(t => t.Ndoc == ndoc, ct);
    }
}
