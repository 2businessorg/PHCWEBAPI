using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

/// <summary>
/// Valida moedas disponíveis a partir de para1 (ge_pteoueuro) e cb.
/// </summary>
public class MoedaRepositoryEFCore : IMoedaRepository
{
    private readonly DossiersDbContextEFCore _context;

    public MoedaRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string moeda, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(moeda)) return false;

        var moedaUpper = moeda.Trim().ToUpperInvariant();

        var moedasPara1 = await _context.Database
            .SqlQueryRaw<string>("SELECT valor AS [Value] FROM para1 WHERE descricao = 'ge_pteoueuro'")
            .ToListAsync(cancellationToken);

        var emPara1 = moedasPara1.Any(x => string.Equals(x?.Trim(), moedaUpper, StringComparison.OrdinalIgnoreCase));

        if (emPara1) return true;

        var moedasCb = await _context.Database
            .SqlQueryRaw<string>("SELECT DISTINCT moeda AS [Value] FROM cb WHERE moeda IS NOT NULL AND moeda <> ''")
            .ToListAsync(cancellationToken);

        var emCb = moedasCb.Any(x => string.Equals(x?.Trim(), moedaUpper, StringComparison.OrdinalIgnoreCase));

        return emCb;
    }

    public async Task<string?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        var moeda = await _context.Database
            .SqlQueryRaw<string>("SELECT TOP 1 valor AS [Value] FROM para1 WHERE descricao = 'ge_pteoueuro'")
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(moeda) ? null : moeda.Trim();
    }
}
