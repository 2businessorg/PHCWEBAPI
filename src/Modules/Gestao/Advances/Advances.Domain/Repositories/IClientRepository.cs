using Advances.Domain.Entities;

namespace Advances.Domain.Repositories;

/// <summary>
/// Repositório de Clientes (para validação no módulo Advances)
/// </summary>
public interface IClientRepository
{
    Task<Cl?> GetByNoAsync(decimal no, CancellationToken ct = default);

    Task<bool> ExistsByNoAsync(decimal no, CancellationToken ct = default);
}
