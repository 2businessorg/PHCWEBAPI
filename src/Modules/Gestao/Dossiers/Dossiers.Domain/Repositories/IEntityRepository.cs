using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório de entidades para operações de dossiers
/// </summary>
public interface IEntityRepository
{
    Task<bool> ExistsByNoAsync(decimal no, CancellationToken cancellationToken = default);
    Task<DossierEntity?> GetByNoAsync(decimal no, CancellationToken cancellationToken = default);
}
