using Advances.Domain.Entities;

namespace Advances.Domain.Repositories;

/// <summary>
/// Repositório de Séries de Adiantamento (tabela tsrd)
/// </summary>
public interface IAdvanceTypeRepository
{
    Task<Tsrd?> GetByNdocAsync(decimal ndoc, CancellationToken ct = default);

    Task<IReadOnlyList<Tsrd>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistsByNdocAsync(decimal ndoc, CancellationToken ct = default);
}
