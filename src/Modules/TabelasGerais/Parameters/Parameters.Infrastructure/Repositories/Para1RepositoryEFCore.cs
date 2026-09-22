using Microsoft.EntityFrameworkCore;
using Parameters.Domain.Entities;
using Parameters.Domain.Repositories;
using Parameters.Infrastructure.Persistence;

namespace Parameters.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório Para1
/// </summary>
public class Para1RepositoryEFCore : IPara1Repository
{
    private readonly ParametersDbContextEFCore _context;

    public Para1RepositoryEFCore(ParametersDbContextEFCore context)
    {
        _context = context;
    }

    public async Task<Para1?> GetByStampAsync(string para1Stamp, CancellationToken cancellationToken = default)
    {
        return await _context.Para1
            .FirstOrDefaultAsync(p => p.Para1Stamp == para1Stamp, cancellationToken);
    }

    public async Task<Para1?> GetByDescricaoAsync(string descricao, CancellationToken cancellationToken = default)
    {
        return await _context.Para1
            .FirstOrDefaultAsync(p => p.Descricao == descricao, cancellationToken);
    }

    public async Task<IEnumerable<Para1>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Para1.AsQueryable();
        return await query.OrderBy(p => p.Descricao).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Para1>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Para1
            .OrderBy(p => p.Descricao)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string para1Stamp, CancellationToken cancellationToken = default)
    {
        return await _context.Para1.AnyAsync(p => p.Para1Stamp == para1Stamp, cancellationToken);
    }

    public async Task<Para1> AddAsync(Para1 para1, CancellationToken cancellationToken = default)
    {
        await _context.Para1.AddAsync(para1, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return para1;
    }

    public async Task UpdateAsync(Para1 para1, CancellationToken cancellationToken = default)
    {
        _context.Para1.Update(para1);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Para1 para1, CancellationToken cancellationToken = default)
    {
        _context.Para1.Remove(para1);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Para1.CountAsync(cancellationToken);
    }
}
