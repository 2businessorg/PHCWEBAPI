using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório de dados complementares de cliente (Cl2)
/// </summary>
public interface ICl2Repository
{
    Task<Cl2?> GetByStampAsync(string stamp, CancellationToken cancellationToken = default);
}
