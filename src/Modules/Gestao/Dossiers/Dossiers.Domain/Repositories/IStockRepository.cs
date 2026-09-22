using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório de artigos/stock para dossiers
/// </summary>
public interface IStockRepository
{
    Task<bool> ExistsByRefAsync(string reference, CancellationToken cancellationToken = default);
    Task<St?> GetByRefAsync(string reference, CancellationToken cancellationToken = default);
    Task<St> AddAsync(St stock, CancellationToken cancellationToken = default);
}
