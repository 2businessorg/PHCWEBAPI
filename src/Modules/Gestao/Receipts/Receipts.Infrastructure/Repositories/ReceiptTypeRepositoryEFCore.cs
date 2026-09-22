using Microsoft.EntityFrameworkCore;
using Receipts.Domain.Entities;
using Receipts.Domain.Repositories;
using Receipts.Infrastructure.Persistence;

namespace Receipts.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de Séries de Recibo (tsre)
/// </summary>
public class ReceiptTypeRepositoryEFCore : IReceiptTypeRepository
{
    private readonly ReceiptsDbContext _db;

    public ReceiptTypeRepositoryEFCore(ReceiptsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<Tsre?> GetByNdocAsync(decimal ndoc, CancellationToken ct = default)
    {
        return await _db.Tsre
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Ndoc == ndoc, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Tsre>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Tsre
            .AsNoTracking()
            .OrderBy(t => t.Ndoc)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNdocAsync(decimal ndoc, CancellationToken ct = default)
    {
        return await _db.Tsre
            .AnyAsync(t => t.Ndoc == ndoc, ct);
    }
}
