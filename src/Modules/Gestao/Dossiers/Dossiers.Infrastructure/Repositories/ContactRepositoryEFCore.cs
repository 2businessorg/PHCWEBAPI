using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

public class ContactRepositoryEFCore : IContactRepository
{
    private readonly DossiersDbContextEFCore _context;

    public ContactRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public Task<bool> ExistsByNoAsync(decimal no, CancellationToken cancellationToken = default)
        => _context.Em.AnyAsync(x => x.No == no, cancellationToken);

    public async Task<DossierEntity?> GetByNoAsync(decimal no, CancellationToken cancellationToken = default)
    {
        var row = await _context.Em
            .Where(x => x.No == no)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null) return null;

        return new DossierEntity
        {
            Stamp = row.Emstamp,
            No = row.No,
            Estab = 0,                // EM não tem Estab
            Nome = row.Nome,
            Ncont = row.Ncont,
            Morada = row.Morada,
            Local = row.Local,
            Codpost = row.Cpostl,     // EM usa Cpostl para código postal
            Segmento = row.Segmento,
            Preco = row.Preco,
            Telefone = row.Telefone,
            Contacto = string.Empty,  // EM não tem campo Contacto único (tem cont1-10)
            Email = row.Email
        };
    }
}
