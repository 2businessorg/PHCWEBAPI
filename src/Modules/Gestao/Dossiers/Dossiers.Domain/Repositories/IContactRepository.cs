using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório de contactos para operações de dossiers
/// </summary>
public interface IContactRepository
{
    Task<bool> ExistsByNoAsync(decimal no, CancellationToken cancellationToken = default);
    Task<DossierEntity?> GetByNoAsync(decimal no, CancellationToken cancellationToken = default);
}
