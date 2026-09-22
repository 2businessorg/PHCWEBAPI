using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

public class SupplierRepositoryEFCore : ISupplierRepository
{
    private readonly DossiersDbContextEFCore _context;

    public SupplierRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public Task<bool> ExistsByNoEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
        => _context.Fl.AnyAsync(x => x.No == no && x.Estab == estab, cancellationToken);

    public async Task<DossierEntity?> GetByNoEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
    {
        var row = await _context.Fl
            .Where(x => x.No == no && x.Estab == estab)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null) return null;

        return new DossierEntity
        {
            Stamp = row.Flstamp,
            No = row.No,
            Estab = row.Estab,
            Nome = row.Nome,
            Ncont = row.Ncont,
            Morada = row.Morada,
            Local = row.Local,
            Codpost = row.Codpost,
            Segmento = string.Empty,  // FL não tem Segmento
            Preco = 0,                 // FL não tem Preco (tabela de preço)
            Telefone = row.Telefone,
            Contacto = row.Contacto,
            Email = row.Email
        };
    }
}
