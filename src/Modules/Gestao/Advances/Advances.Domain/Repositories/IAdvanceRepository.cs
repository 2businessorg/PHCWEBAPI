using Advances.Domain.Entities;

namespace Advances.Domain.Repositories;

/// <summary>
/// Repositório de Adiantamentos (tabela rd)
/// </summary>
public interface IAdvanceRepository
{
    Task<Rd?> GetByKeyAsync(decimal ndoc, decimal rno, decimal rdano, CancellationToken ct = default);

    Task<(int TotalItems, IReadOnlyList<Rd> Items)> GetAllAsync(
        decimal? ndoc, decimal? rno, decimal? rdano, decimal? no,
        int page, int pageSize, CancellationToken ct = default);

    Task<string> GetBankAccountNameAsync(decimal noconta, CancellationToken ct = default);

    Task<bool> ExistsByKeyAsync(decimal ndoc, decimal rno, decimal rdano, CancellationToken ct = default);

    Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string rdstamp, IReadOnlyList<string> columns, CancellationToken ct = default);

    Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(
        IEnumerable<string> rdstamps, IReadOnlyList<string> columns, CancellationToken ct = default);
}
