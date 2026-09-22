using Treasury.Domain.Entities;

namespace Treasury.Domain.Repositories;

/// <summary>
/// Read-only persistence for bank reconciliation movements (PHC <c>ba</c> / <c>br</c>).
/// </summary>
public interface IBankReconciliationRepository
{
    /// <summary>
    /// Returns unreconciled imported bank movements for an account and date window.
    /// </summary>
    Task<(int TotalCount, IReadOnlyList<ImportedBankMovement> Items)> GetImportedBankMovementsAsync(
        decimal accountCode,
        DateTime dateFrom,
        DateTime dateToExclusive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns unreconciled treasury account movements for an account and date window.
    /// </summary>
    Task<(int TotalCount, IReadOnlyList<TreasuryAccountMovement> Items)> GetTreasuryAccountMovementsAsync(
        decimal accountCode,
        DateTime dateFrom,
        DateTime dateToExclusive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
