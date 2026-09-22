using Receipts.Domain.Entities;

namespace Receipts.Domain.Repositories;

/// <summary>
/// Repositório de Clientes — validação de existência para recibos
/// </summary>
public interface IClientRepository
{
    Task<Cl?> GetByNoAsync(decimal no, CancellationToken ct = default);
    Task<bool> ExistsByNoAsync(decimal no, CancellationToken ct = default);
}
