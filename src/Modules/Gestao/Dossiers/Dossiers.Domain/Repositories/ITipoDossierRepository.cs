using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório de tipos de dossier
/// </summary>
public interface ITipoDossierRepository
{
    Task<bool> ExistsByIdAsync(decimal ndos, CancellationToken cancellationToken = default);
    Task<Ts?> GetByIdAsync(decimal ndos, CancellationToken cancellationToken = default);
    Task<List<Ts>> GetAllAsync(CancellationToken cancellationToken = default);
}
