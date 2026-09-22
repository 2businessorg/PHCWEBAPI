using Microsoft.EntityFrameworkCore;
using Providers.Domain.Entities;
using Providers.Domain.Repositories;
using Providers.Infrastructure.Persistence;

namespace Providers.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório Provider
/// </summary>
public class ProviderRepositoryEFCore : IProviderRepository
{
    private readonly ProvidersDbContextEFCore _context;

    public ProviderRepositoryEFCore(ProvidersDbContextEFCore context)
    {
        _context = context;
    }

    public async Task<Provider?> GetByStampAsync(string uProviderStamp, CancellationToken cancellationToken = default)
    {
        return await _context.Providers
            .Include(p => p.Values)
            .FirstOrDefaultAsync(p => p.UProviderStamp == uProviderStamp, cancellationToken);
    }

    public async Task<Provider?> GetByCodigoAsync(int codigo, CancellationToken cancellationToken = default)
    {
        return await _context.Providers
            .Include(p => p.Values)
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
    }

    public async Task<Provider?> GetByProviderAndEnvironmentAsync(string provedor, string environment, CancellationToken cancellationToken = default)
    {
        return await _context.Providers
            .Include(p => p.Values)
            .FirstOrDefaultAsync(p => p.Provedor == provedor && p.Environment == environment, cancellationToken);
    }

    public async Task<IEnumerable<Provider>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Providers
            .Include(p => p.Values)  // ✅ Carregar linhas (Values)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.Ativo);

        return await query.OrderBy(p => p.Provedor).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Provider>> GetByEnvironmentAsync(string environment, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Providers
            .Include(p => p.Values)  // ✅ Carregar linhas (Values)
            .Where(p => p.Environment == environment);

        if (!includeInactive)
            query = query.Where(p => p.Ativo);

        return await query.OrderBy(p => p.Provedor).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Provider>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Providers
            .OrderBy(p => p.Provedor)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string uProviderStamp, CancellationToken cancellationToken = default)
    {
        return await _context.Providers.AnyAsync(p => p.UProviderStamp == uProviderStamp, cancellationToken);
    }

    public async Task<bool> ExistsProviderEnvironmentAsync(string provedor, string environment, CancellationToken cancellationToken = default)
    {
        return await _context.Providers.AnyAsync(p => p.Provedor == provedor && p.Environment == environment, cancellationToken);
    }

    public async Task<Provider> AddAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        await _context.Providers.AddAsync(provider, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return provider;
    }

    public async Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        _context.Providers.Update(provider);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        _context.Providers.Remove(provider);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Providers.CountAsync(cancellationToken);
    }
}
