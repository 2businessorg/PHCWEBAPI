using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

public class EntityRepositoryEFCore : IEntityRepository
{
    private readonly DossiersDbContextEFCore _context;

    public EntityRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public Task<bool> ExistsByNoAsync(decimal no, CancellationToken cancellationToken = default)
        => _context.Ag.AnyAsync(x => x.No == no, cancellationToken);

    public async Task<DossierEntity?> GetByNoAsync(decimal no, CancellationToken cancellationToken = default)
    {
        var row = await _context.Ag
            .Where(x => x.No == no)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null) return null;

        return new DossierEntity
        {
            Stamp = row.Agstamp,
            No = row.No,
            Estab = 0,                // AG não tem Estab
            Nome = row.Nome,
            Ncont = row.Ncont,
            Morada = row.Morada,
            Local = row.Local,
            Codpost = row.Codpost,
            Segmento = string.Empty,  // AG não tem Segmento
            Preco = 0,                 // AG não tem Preco
            Telefone = row.Telefone,
            Contacto = row.Contacto,
            Email = row.Email
        };
    }
}
