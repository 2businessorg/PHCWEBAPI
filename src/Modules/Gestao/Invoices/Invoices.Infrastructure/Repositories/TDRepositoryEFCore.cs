using Invoices.Domain.Entities;
using Invoices.Domain.Repositories;
using Invoices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Invoices.Infrastructure.Repositories;

/// <summary>
/// Repositório EF Core para tipos de documento (TD)
/// </summary>
public class TDRepositoryEFCore : ITDRepository
{
    private readonly FaturasDbContext _context;

    public TDRepositoryEFCore(FaturasDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifica se um tipo de documento existe pelo ID
    /// </summary>
    public Task<bool> ExistsByIdAsync(decimal ndoc, CancellationToken cancellationToken = default)
        => _context.TD.AnyAsync(x => x.Ndoc == ndoc, cancellationToken);

    /// <summary>
    /// Obtém um tipo de documento pelo ID
    /// </summary>
    public Task<TD?> GetByIdAsync(decimal ndoc, CancellationToken cancellationToken = default)
        => _context.TD.FirstOrDefaultAsync(x => x.Ndoc == ndoc, cancellationToken);

    /// <summary>
    /// Obtém todos os tipos de documento
    /// </summary>
    public Task<List<TD>> GetAllAsync(CancellationToken cancellationToken = default)
        => _context.TD
            .AsNoTracking()
            .OrderBy(x => x.Ndoc)
            .Select(x => new TD
            {
                Ndoc = x.Ndoc,
                NmDoc = (x.NmDoc ?? string.Empty).Trim(),
                NmDocP = (x.NmDocP ?? string.Empty).Trim(),
                NmDocA = (x.NmDocA ?? string.Empty).Trim(),
                Serie = x.Serie,
                GuiaRemessa = x.GuiaRemessa,
                AutoFat = x.AutoFat,
                LancaCc = x.LancaCc,
                LancaSl = x.LancaSl,
                LancaOl = x.LancaOl,
                AutoMl = x.AutoMl,
                Fechada = x.Fechada,
                ExcluiSaft = x.ExcluiSaft,
                LimiteSimp = x.LimiteSimp,
                QttDec = x.QttDec,
                PreDec = x.PreDec,
                OusrData = x.OusrData,
                OusrHora = x.OusrHora,
                OusrInis = x.OusrInis,
                AspNetUsersId = x.AspNetUsersId
            })
            .ToListAsync(cancellationToken);
}
