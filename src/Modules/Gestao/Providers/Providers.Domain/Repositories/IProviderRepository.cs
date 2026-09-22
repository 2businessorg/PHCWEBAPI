using Providers.Domain.Entities;

namespace Providers.Domain.Repositories;

/// <summary>
/// Interface do repositório de Providers
/// </summary>
public interface IProviderRepository
{
    Task<Provider?> GetByStampAsync(string uProviderStamp, CancellationToken cancellationToken = default);
    Task<Provider?> GetByCodigoAsync(int codigo, CancellationToken cancellationToken = default);
    Task<Provider?> GetByProviderAndEnvironmentAsync(string provedor, string environment, CancellationToken cancellationToken = default);
    Task<IEnumerable<Provider>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<Provider>> GetByEnvironmentAsync(string environment, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<Provider>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string uProviderStamp, CancellationToken cancellationToken = default);
    Task<bool> ExistsProviderEnvironmentAsync(string provedor, string environment, CancellationToken cancellationToken = default);
    Task<Provider> AddAsync(Provider provider, CancellationToken cancellationToken = default);
    Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default);
    Task DeleteAsync(Provider provider, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
