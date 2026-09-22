using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório de fornecedores para operações de dossiers
/// </summary>
public interface ISupplierRepository
{
    Task<bool> ExistsByNoEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);
    Task<DossierEntity?> GetByNoEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);
}
