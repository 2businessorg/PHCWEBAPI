using Treasury.Application.DTOs;

namespace Treasury.Application.Mappings;

/// <summary>
/// Precomputed money totals so API and MCP consumers do not have to sum line items.
/// </summary>
public readonly record struct ReconciliationTotals(
    decimal BankAmountTotal,
    decimal BankCreditTotal,
    decimal BankDebitTotal,
    decimal TreasuryInflowTotal,
    decimal TreasuryOutflowTotal,
    decimal TreasuryNetTotal)
{
    public static ReconciliationTotals From(
        IReadOnlyList<ImportedBankMovementOutputDTO> bankMovements,
        IReadOnlyList<TreasuryAccountMovementOutputDTO> treasuryMovements)
    {
        decimal bankAmount = 0;
        decimal bankCredit = 0;
        decimal bankDebit = 0;
        foreach (var movement in bankMovements)
        {
            bankAmount += movement.Amount;
            if (movement.Amount >= 0)
            {
                bankCredit += movement.Amount;
            }
            else
            {
                bankDebit += -movement.Amount;
            }
        }

        decimal inflow = 0;
        decimal outflow = 0;
        foreach (var movement in treasuryMovements)
        {
            inflow += movement.Inflow;
            outflow += movement.Outflow;
        }

        return new ReconciliationTotals(
            bankAmount,
            bankCredit,
            bankDebit,
            inflow,
            outflow,
            inflow - outflow);
    }
}
