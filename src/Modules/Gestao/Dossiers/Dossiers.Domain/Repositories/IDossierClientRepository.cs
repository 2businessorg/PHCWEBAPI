using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório de clientes para operações de dossiers
/// </summary>
public interface IDossierClientRepository
{
    Task<bool> ExistsByNoEstabEntityAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);
    Task<DossierEntity?> GetByNoEstabEntityAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);
}
