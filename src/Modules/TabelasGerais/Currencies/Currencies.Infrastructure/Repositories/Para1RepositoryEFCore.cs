using Currencies.Domain.Entities;
using Currencies.Domain.Repositories;
using Currencies.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Currencies.Infrastructure.Repositories;

/// <summary>
/// Repositório EF Core da tabela para1.
/// </summary>
public sealed class Para1RepositoryEFCore : IPara1Repository
{
    private readonly CurrenciesDbContext _context;

    public Para1RepositoryEFCore(CurrenciesDbContext context)
    {
        _context = context;
    }

    public async Task<Para1?> GetByDescricaoAsync(string descricao, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return null;

        return await _context.Para1
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Descricao == descricao, cancellationToken);
    }
}
