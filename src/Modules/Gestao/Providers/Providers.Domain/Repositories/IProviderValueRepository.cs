using Providers.Domain.Entities;

namespace Providers.Domain.Repositories;

/// <summary>
/// Interface do repositório de ProviderValues
/// </summary>
public interface IProviderValueRepository
{
    Task<ProviderValue?> GetByStampAsync(string uProviderValuesStamp, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderValue>> GetByProviderStampAsync(string uProviderStamp, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderValue>> GetByProviderAndOperationAsync(string uProviderStamp, string operationCode, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ProviderValue?> GetByProviderOperationAndKeyAsync(string uProviderStamp, string operationCode, string chave, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderValue>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string uProviderValuesStamp, CancellationToken cancellationToken = default);
    Task<bool> ExistsKeyAsync(string uProviderStamp, string operationCode, string chave, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove todos os valores de um Provider (usado no Update para replace completo)
    /// </summary>
    Task DeleteByProviderStampAsync(string uProviderStamp, CancellationToken cancellationToken = default);
    
    Task<ProviderValue> AddAsync(ProviderValue providerValue, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProviderValue providerValue, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProviderValue providerValue, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
