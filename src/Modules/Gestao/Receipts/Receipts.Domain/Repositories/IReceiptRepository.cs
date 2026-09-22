using Receipts.Domain.Entities;

namespace Receipts.Domain.Repositories;

/// <summary>
/// Repositório de Recibos (tabelas re + rl)
/// </summary>
public interface IReceiptRepository
{
    Task<ReceiptAggregate?> GetByKeyAsync(decimal ndoc, decimal rno, decimal reano, CancellationToken ct = default);
    Task<(int TotalItems, IReadOnlyList<Re> Items)> GetAllAsync(
        decimal? ndoc,
        decimal? rno,
        decimal? reano,
        decimal? no,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<List<Rl>> GetLinesByRestampAsync(string restamp, CancellationToken ct = default);
    Task<string> GetBankAccountNameAsync(decimal noconta, CancellationToken ct = default);
    Task<string> GetInvoiceSeriesIdAsync(string ccstamp, CancellationToken ct = default);
    Task<bool> ExistsByKeyAsync(decimal ndoc, decimal rno, decimal reano, CancellationToken ct = default);

    // User fields (addFields) — tabela re
    Task<Dictionary<string, object?>> GetUserFieldValuesAsync(string restamp, IReadOnlyList<string> columns, CancellationToken ct = default);
    Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(IEnumerable<string> restamps, IReadOnlyList<string> columns, CancellationToken ct = default);

    // User fields (addFields) — tabela rl
    Task<Dictionary<string, object?>> GetUserFieldValuesRlAsync(string rlstamp, IReadOnlyList<string> columns, CancellationToken ct = default);
    Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesRlBatchAsync(IEnumerable<string> rlstamps, IReadOnlyList<string> columns, CancellationToken ct = default);
}
