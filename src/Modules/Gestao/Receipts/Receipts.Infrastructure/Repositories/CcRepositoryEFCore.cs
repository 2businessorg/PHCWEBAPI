using Microsoft.EntityFrameworkCore;
using Receipts.Domain.Entities;
using Receipts.Domain.Repositories;
using Receipts.Infrastructure.Persistence;

namespace Receipts.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de Conta Corrente (cc)
/// </summary>
public class CcRepositoryEFCore : ICcRepository
{
    private readonly ReceiptsDbContext _db;

    public CcRepositoryEFCore(ReceiptsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    /// <remarks>
    /// O ccstamp em cc é o ftstamp da factura correspondente.
    /// A resolução é feita por nrdoc + ano + serie (+ ndoc opcional) e no do cliente.
    /// </remarks>
    public async Task<Cc?> GetByInvoiceAsync(
        decimal no,
        decimal nrdoc,
        decimal ano,
        string serie,
        decimal? ndoc = null,
        CancellationToken ct = default)
    {
        var query =
            from c in _db.Cc.AsNoTracking()
            join f in _db.Ft.AsNoTracking() on c.Ccstamp equals f.Ftstamp
            where c.No == no
               && c.Nrdoc == nrdoc
               && f.Ftano == ano
               && f.Series == serie
            select new { Cc = c, Ft = f };

        if (ndoc.HasValue)
            query = query.Where(x => x.Ft.Ndoc == ndoc.Value);

        var result = await query.FirstOrDefaultAsync(ct);
        return result?.Cc;
    }
}
