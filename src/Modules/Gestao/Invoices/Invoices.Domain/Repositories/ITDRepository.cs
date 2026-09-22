using Invoices.Domain.Entities;

namespace Invoices.Domain.Repositories;

/// <summary>
/// Repositório de tipos de documento (TD - Document Types Configuration)
/// </summary>
public interface ITDRepository
{
    /// <summary>
    /// Verifica se um tipo de documento existe pelo ID
    /// </summary>
    Task<bool> ExistsByIdAsync(decimal ndoc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um tipo de documento pelo ID
    /// </summary>
    Task<TD?> GetByIdAsync(decimal ndoc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os tipos de documento
    /// </summary>
    Task<List<TD>> GetAllAsync(CancellationToken cancellationToken = default);
}
