using Treasury.Application.DTOs;

namespace Treasury.Application.Matching;

/// <summary>
/// Suggests BR/BA combinations without writing <c>reco</c>.
/// </summary>
public interface IReconciliationMatcher
{
    IReadOnlyList<ReconciliationMatchCombinationDTO> Match(
        IReadOnlyList<ImportedBankMovementOutputDTO> bankMovements,
        IReadOnlyList<TreasuryAccountMovementOutputDTO> treasuryMovements);
}
