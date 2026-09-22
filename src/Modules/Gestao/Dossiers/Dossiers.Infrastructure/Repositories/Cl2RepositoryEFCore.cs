using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

public class Cl2RepositoryEFCore : ICl2Repository
{
    private readonly DossiersDbContextEFCore _context;

    public Cl2RepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public Task<Cl2?> GetByStampAsync(string stamp, CancellationToken cancellationToken = default)
        => _context.Cl2.FirstOrDefaultAsync(x => x.Cl2stamp == stamp, cancellationToken);
}
