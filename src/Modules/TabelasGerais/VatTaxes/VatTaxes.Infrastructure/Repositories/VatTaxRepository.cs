using Microsoft.EntityFrameworkCore;
using VatTaxes.Domain;
using VatTaxes.Infrastructure.Persistence;

namespace VatTaxes.Infrastructure.Repositories;

/// <summary>
/// Repository para gestão de Taxas de IVA (TaxasIva)
/// </summary>
public class VatTaxRepository : IVatTaxRepository
{
    private readonly VatTaxesDbContext _context;

    public VatTaxRepository(VatTaxesDbContext context)
    {
        _context = context;
    }

    public async Task<TaxasIva?> GetByCodeAsync(int code, CancellationToken cancellationToken = default)
    {
        return await _context.TaxasIva
            .FirstOrDefaultAsync(v => v.Codigo == code, cancellationToken);
    }

    public async Task<TaxasIva?> GetByStampAsync(string stamp, CancellationToken cancellationToken = default)
    {
        return await _context.TaxasIva
            .FirstOrDefaultAsync(v => v.TaxasIvaStamp == stamp, cancellationToken);
    }

    public async Task<List<TaxasIva>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TaxasIva
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TaxasIva>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.TaxasIva
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TaxasIva.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(int code, CancellationToken cancellationToken = default)
    {
        return await _context.TaxasIva
            .AnyAsync(v => v.Codigo == code, cancellationToken);
    }

    public async Task AddAsync(TaxasIva taxasIva, CancellationToken cancellationToken = default)
    {
        await _context.TaxasIva.AddAsync(taxasIva, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaxasIva taxasIva, CancellationToken cancellationToken = default)
    {
        _context.TaxasIva.Update(taxasIva);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByCodeAsync(int code, CancellationToken cancellationToken = default)
    {
        var taxasIva = await GetByCodeAsync(code, cancellationToken);
        if (taxasIva != null)
        {
            _context.TaxasIva.Remove(taxasIva);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<decimal> GetRateByCodeAsync(int code, CancellationToken cancellationToken = default)
    {
        var taxasIva = await GetByCodeAsync(code, cancellationToken);
        return taxasIva?.Taxa ?? 0;
    }
}
