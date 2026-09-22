using Treasury.Application.DTOs;

namespace Treasury.Application.Mappings;

/// <summary>
/// Maps a full movements result to a compact summary for small models.
/// </summary>
public static class ReconciliationSummaryMapper
{
    public static ReconciliationSummaryOutputDTO From(ReconciliationMovementsOutputDTO movements)
        => new()
        {
            Account = movements.Account,
            Period = movements.Period,
            BankMovementCount = movements.BankMovementTotalCount,
            TreasuryMovementCount = movements.TreasuryMovementTotalCount,
            BankAmountTotal = movements.BankAmountTotal,
            BankCreditTotal = movements.BankCreditTotal,
            BankDebitTotal = movements.BankDebitTotal,
            TreasuryInflowTotal = movements.TreasuryInflowTotal,
            TreasuryOutflowTotal = movements.TreasuryOutflowTotal,
            TreasuryNetTotal = movements.TreasuryNetTotal,
            TotalsAreComplete =
                movements.BankMovementCount >= movements.BankMovementTotalCount
                && movements.TreasuryMovementCount >= movements.TreasuryMovementTotalCount
        };
}
