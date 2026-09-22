using Microsoft.EntityFrameworkCore;
using Receipts.Domain.Entities;
using Receipts.Domain.Repositories;
using Receipts.Infrastructure.Persistence;

namespace Receipts.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de Clientes (para validação no módulo Receipts)
/// </summary>
public class ClientRepositoryEFCore : IClientRepository
{
    private readonly ReceiptsDbContext _db;

    public ClientRepositoryEFCore(ReceiptsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<Cl?> GetByNoAsync(decimal no, CancellationToken ct = default)
    {
        return await _db.Cl
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.No == no && c.Estab == 0, ct);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNoAsync(decimal no, CancellationToken ct = default)
    {
        return await _db.Cl
            .AnyAsync(c => c.No == no && c.Estab == 0, ct);
    }
}
