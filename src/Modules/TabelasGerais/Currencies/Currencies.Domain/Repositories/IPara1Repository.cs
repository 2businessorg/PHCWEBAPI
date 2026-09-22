using Currencies.Domain.Entities;

namespace Currencies.Domain.Repositories;

/// <summary>
/// Repositório da tabela para1.
/// </summary>
public interface IPara1Repository
{
    /// <summary>
    /// Obtém um registo por descrição.
    /// </summary>
    Task<Para1?> GetByDescricaoAsync(string descricao, CancellationToken cancellationToken = default);
}
