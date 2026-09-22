using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

public class DossierClientRepositoryEFCore : IDossierClientRepository
{
    private readonly DossiersDbContextEFCore _context;

    public DossierClientRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public Task<bool> ExistsByNoEstabEntityAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
        => _context.Cl.AnyAsync(x => x.No == no && x.Estab == estab, cancellationToken);

    public async Task<DossierEntity?> GetByNoEstabEntityAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
    {
        var row = await _context.Cl
            .Where(x => x.No == no && x.Estab == estab)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null) return null;

        return new DossierEntity
        {
            Stamp = row.Clstamp,
            No = row.No,
            Estab = row.Estab,
            Nome = row.Nome,
            Ncont = row.Ncont,
            Morada = row.Morada,
            Local = row.Local,
            Codpost = row.Codpost,
            Segmento = row.Segmento,
            Preco = row.Preco,
            Telefone = row.Telefone,
            Contacto = row.Contacto,
            Email = row.Email
        };
    }
}
