using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

public class TaxasIvaRepositoryEFCore : ITaxasIvaRepository
{
    private readonly DossiersDbContextEFCore _context;

    public TaxasIvaRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public Task<Taxasiva?> GetByTaxRateAsync(decimal taxRate, CancellationToken cancellationToken = default)
        => _context.Taxasiva.FirstOrDefaultAsync(x => x.Taxa == taxRate, cancellationToken);

    public Task<Taxasiva?> GetByCodeAsync(int code, CancellationToken cancellationToken = default)
        => _context.Taxasiva.FirstOrDefaultAsync(x => x.Codigo == code, cancellationToken);

    public async Task<decimal> GetTaxRateByCodeAsync(int code, CancellationToken cancellationToken = default)
    {
        var row = await _context.Taxasiva.FirstOrDefaultAsync(x => x.Codigo == code, cancellationToken);
        return row?.Taxa ?? 0m;
    }

    public (decimal BaseInc, decimal TaxValue) Calculate(int code, bool ivaIncluded, decimal grossAmount, decimal taxRateOverride = -1m)
    {
        var rate = taxRateOverride >= 0 ? taxRateOverride : 0m;
        if (rate <= 0)
        {
            // fallback para código sem taxa conhecida
            return (Math.Round(grossAmount, 6), 0m);
        }

        if (!ivaIncluded)
        {
            var baseInc = Math.Round(grossAmount, 6);
            var tax = Math.Round(baseInc * rate / 100m, 6);
            return (baseInc, tax);
        }

        var divisor = 1m + (rate / 100m);
        var baseIncluded = divisor > 0 ? Math.Round(grossAmount / divisor, 6) : Math.Round(grossAmount, 6);
        var taxIncluded = Math.Round(grossAmount - baseIncluded, 6);

        return (baseIncluded, taxIncluded);
    }
}
