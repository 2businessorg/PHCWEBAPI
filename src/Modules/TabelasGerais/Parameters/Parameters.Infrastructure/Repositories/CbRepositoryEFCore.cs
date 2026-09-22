using Microsoft.EntityFrameworkCore;
using Parameters.Domain.Repositories;
using Parameters.Infrastructure.Persistence;

namespace Parameters.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core de ICbRepository.
/// Lista de moedas resulta do union:
///   SELECT valor FROM para1 WHERE descricao = 'ge_pteoueuro'
///   UNION ALL
///   SELECT DISTINCT moeda FROM cb
/// </summary>
public class CbRepositoryEFCore : ICbRepository
{
    private readonly ParametersDbContextEFCore _context;

    public CbRepositoryEFCore(ParametersDbContextEFCore context)
    {
        _context = context;
    }

    public async Task<IEnumerable<string>> GetMoedasAsync(CancellationToken cancellationToken = default)
    {
        // moeda base da configuração (para1)
        var moedaBase = await _context.Para1
            .Where(p => p.Descricao == "ge_pteoueuro")
            .Select(p => p.Valor)
            .FirstOrDefaultAsync(cancellationToken);

        // moedas distintas da tabela cb
        var moedasCb = await _context.Cb
            .Where(c => !string.IsNullOrEmpty(c.Moeda))
            .Select(c => c.Moeda)
            .Distinct()
            .ToListAsync(cancellationToken);

        var result = new List<string>();

        if (!string.IsNullOrWhiteSpace(moedaBase))
            result.Add(moedaBase);

        result.AddRange(moedasCb.Where(m => !result.Contains(m)));

        return result.OrderBy(m => m);
    }

    public async Task<bool> ExistsMoedaAsync(string moeda, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(moeda)) return false;

        var moedaUpper = moeda.Trim().ToUpperInvariant();

        var emPara1 = await _context.Para1
            .AnyAsync(p => p.Descricao == "ge_pteoueuro" &&
                           p.Valor.ToUpper() == moedaUpper, cancellationToken);

        if (emPara1) return true;

        return await _context.Cb
            .AnyAsync(c => c.Moeda.ToUpper() == moedaUpper, cancellationToken);
    }
}
