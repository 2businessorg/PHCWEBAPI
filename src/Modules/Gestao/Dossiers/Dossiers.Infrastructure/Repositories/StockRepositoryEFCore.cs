using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dossiers.Infrastructure.Repositories;

public class StockRepositoryEFCore : IStockRepository
{
    private readonly DossiersDbContextEFCore _context;

    public StockRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public Task<bool> ExistsByRefAsync(string reference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reference)) return Task.FromResult(false);
        return _context.St.AnyAsync(x => x.Ref == reference, cancellationToken);
    }

    public Task<St?> GetByRefAsync(string reference, CancellationToken cancellationToken = default)
        => _context.St.FirstOrDefaultAsync(x => x.Ref == reference, cancellationToken);

    public async Task<St> AddAsync(St stock, CancellationToken cancellationToken = default)
    {
        _context.St.Add(stock);
        await _context.SaveChangesAsync(cancellationToken);
        return stock;
    }
}
