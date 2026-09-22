using Invoices.Domain.Entities;

namespace Invoices.Domain.Repositories;

/// <summary>
/// Interface do repositório para produtos/artigos (ST)
/// Dados mínimos necessários para validação de faturas
/// </summary>
public interface IStockRepository
{
    /// <summary>
    /// Verifica se um artigo existe pela sua referência
    /// </summary>
    Task<bool> ExistsByRefAsync(string ref_code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um artigo pela sua referência
    /// </summary>
    Task<St?> GetByRefAsync(string ref_code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo artigo
    /// </summary>
    Task AddAsync(St artigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um artigo existente
    /// </summary>
    Task UpdateAsync(St artigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um artigo
    /// </summary>
    Task<bool> DeleteAsync(string ref_code, CancellationToken cancellationToken = default);
}
