using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

public class TipoDossierRepositoryEFCore : ITipoDossierRepository
{
    private readonly DossiersDbContextEFCore _context;

    public TipoDossierRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public Task<bool> ExistsByIdAsync(decimal ndos, CancellationToken cancellationToken = default)
        => _context.Ts.AnyAsync(x => x.Ndos == ndos, cancellationToken);

    public Task<Ts?> GetByIdAsync(decimal ndos, CancellationToken cancellationToken = default)
        => _context.Ts.FirstOrDefaultAsync(x => x.Ndos == ndos, cancellationToken);

    public Task<List<Ts>> GetAllAsync(CancellationToken cancellationToken = default)
        => _context.Ts
            .AsNoTracking()
            .OrderBy(x => x.Ndos)
            .Select(x => new Ts
            {
                Ndos = x.Ndos,
                Nmdos = (x.Nmdos ?? string.Empty).Trim(),
                Bdempresas = (x.Bdempresas ?? string.Empty).Trim()
            })
            .ToListAsync(cancellationToken);
}
