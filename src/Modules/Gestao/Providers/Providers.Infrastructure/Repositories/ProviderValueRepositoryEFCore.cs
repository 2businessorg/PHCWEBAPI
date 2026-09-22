using Microsoft.EntityFrameworkCore;
using Providers.Domain.Entities;
using Providers.Domain.Repositories;
using Providers.Infrastructure.Persistence;

namespace Providers.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório ProviderValue
/// </summary>
public class ProviderValueRepositoryEFCore : IProviderValueRepository
{
    private readonly ProvidersDbContextEFCore _context;

    public ProviderValueRepositoryEFCore(ProvidersDbContextEFCore context)
    {
        _context = context;
    }

    public async Task<ProviderValue?> GetByStampAsync(string uProviderValuesStamp, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderValues
            .FirstOrDefaultAsync(pv => pv.UProviderValuesStamp == uProviderValuesStamp, cancellationToken);
    }

    public async Task<IEnumerable<ProviderValue>> GetByProviderStampAsync(string uProviderStamp, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ProviderValues.Where(pv => pv.UProviderStamp == uProviderStamp);

        if (!includeInactive)
            query = query.Where(pv => pv.Ativo);

        return await query.OrderBy(pv => pv.Ordem).ThenBy(pv => pv.OperationCode).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProviderValue>> GetByProviderAndOperationAsync(string uProviderStamp, string operationCode, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ProviderValues
            .Where(pv => pv.UProviderStamp == uProviderStamp && pv.OperationCode == operationCode);

        if (!includeInactive)
            query = query.Where(pv => pv.Ativo);

        return await query.OrderBy(pv => pv.Ordem).ToListAsync(cancellationToken);
    }

    public async Task<ProviderValue?> GetByProviderOperationAndKeyAsync(string uProviderStamp, string operationCode, string chave, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderValues
            .FirstOrDefaultAsync(pv => pv.UProviderStamp == uProviderStamp 
                && pv.OperationCode == operationCode 
                && pv.Chave == chave, cancellationToken);
    }

    public async Task<IEnumerable<ProviderValue>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ProviderValues.AsQueryable();

        if (!includeInactive)
            query = query.Where(pv => pv.Ativo);

        return await query.OrderBy(pv => pv.UProviderStamp).ThenBy(pv => pv.Ordem).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string uProviderValuesStamp, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderValues.AnyAsync(pv => pv.UProviderValuesStamp == uProviderValuesStamp, cancellationToken);
    }

    public async Task<bool> ExistsKeyAsync(string uProviderStamp, string operationCode, string chave, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderValues.AnyAsync(pv => 
            pv.UProviderStamp == uProviderStamp 
            && pv.OperationCode == operationCode 
            && pv.Chave == chave, cancellationToken);
    }

    public async Task DeleteByProviderStampAsync(string uProviderStamp, CancellationToken cancellationToken = default)
    {
        var values = await _context.ProviderValues
            .Where(pv => pv.UProviderStamp == uProviderStamp)
            .ToListAsync(cancellationToken);

        _context.ProviderValues.RemoveRange(values);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProviderValue> AddAsync(ProviderValue providerValue, CancellationToken cancellationToken = default)
    {
        await _context.ProviderValues.AddAsync(providerValue, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return providerValue;
    }

    public async Task UpdateAsync(ProviderValue providerValue, CancellationToken cancellationToken = default)
    {
        _context.ProviderValues.Update(providerValue);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ProviderValue providerValue, CancellationToken cancellationToken = default)
    {
        _context.ProviderValues.Remove(providerValue);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProviderValues.CountAsync(cancellationToken);
    }
}
