using Advances.Domain.Entities;
using Advances.Domain.Repositories;
using Advances.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Advances.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de Clientes (para validação no módulo Advances)
/// </summary>
public class ClientRepositoryEFCore : IClientRepository
{
    private readonly AdvancesDbContext _db;

    public ClientRepositoryEFCore(AdvancesDbContext db)
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
