using Invoices.Domain.Entities;

namespace Invoices.Domain.Repositories;

/// <summary>
/// Interface do repositório para clientes (CL)
/// Dados mínimos necessários para validação de faturas
/// </summary>
public interface IClientRepository
{
    /// <summary>
    /// Verifica se um cliente existe pela combinação No + Estab
    /// </summary>
    Task<bool> ExistsByNoAndEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um cliente pela combinação No + Estab
    /// </summary>
    Task<Cl?> GetByNoAndEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo cliente
    /// </summary>
    Task AddAsync(Cl cliente, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    Task UpdateAsync(Cl cliente, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um cliente
    /// </summary>
    Task<bool> DeleteAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);
}
