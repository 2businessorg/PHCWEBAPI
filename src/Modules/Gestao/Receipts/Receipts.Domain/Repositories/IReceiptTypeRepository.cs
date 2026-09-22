using Receipts.Domain.Entities;

namespace Receipts.Domain.Repositories;

/// <summary>
/// Repositório de Séries de Recibo (tabela tsre)
/// </summary>
public interface IReceiptTypeRepository
{
    Task<Tsre?> GetByNdocAsync(decimal ndoc, CancellationToken ct = default);
    Task<IReadOnlyList<Tsre>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNdocAsync(decimal ndoc, CancellationToken ct = default);
}
