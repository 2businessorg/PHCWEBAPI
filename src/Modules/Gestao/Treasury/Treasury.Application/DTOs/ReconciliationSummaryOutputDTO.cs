namespace Treasury.Application.DTOs;

/// <summary>
/// Compact reconciliation totals for small models and summary REST consumers.
/// Does not include movement line items.
/// </summary>
public record ReconciliationSummaryOutputDTO
{
    public TreasuryAccountOutputDTO Account { get; init; } = new();

    public ReconciliationPeriodOutputDTO Period { get; init; } = new();

    public int BankMovementCount { get; init; }

    public int TreasuryMovementCount { get; init; }

    public decimal BankAmountTotal { get; init; }

    public decimal BankCreditTotal { get; init; }

    public decimal BankDebitTotal { get; init; }

    public decimal TreasuryInflowTotal { get; init; }

    public decimal TreasuryOutflowTotal { get; init; }

    public decimal TreasuryNetTotal { get; init; }

    /// <summary>
    /// False when the page used to compute money totals is smaller than the full count.
    /// </summary>
    public bool TotalsAreComplete { get; init; } = true;

    /// <summary>Guidance for language models: use these totals, do not re-sum lines.</summary>
    public string Instruction { get; init; } =
        "Usa estes totais e contagens. Nao somes linhas. Nao inventes valores. Nao trates prefixos de stamp (AON, ANM, SDO, KFU) como nomes de empresas.";
}
