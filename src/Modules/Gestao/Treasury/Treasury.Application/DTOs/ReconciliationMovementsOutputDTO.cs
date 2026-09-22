namespace Treasury.Application.DTOs;

/// <summary>
/// Unreconciled imported bank movements and treasury account movements for one account and period.
/// Pagination fields are present so a later version can page large windows without changing the contract.
/// </summary>
public record ReconciliationMovementsOutputDTO
{
    /// <summary>Resolved treasury account.</summary>
    public TreasuryAccountOutputDTO Account { get; init; } = new();

    /// <summary>Requested inclusive period.</summary>
    public ReconciliationPeriodOutputDTO Period { get; init; } = new();

    /// <summary>Imported bank movements (PHC <c>br</c>).</summary>
    public IReadOnlyList<ImportedBankMovementOutputDTO> BankMovements { get; init; } =
        Array.Empty<ImportedBankMovementOutputDTO>();

    /// <summary>Treasury account movements (PHC <c>ba</c>).</summary>
    public IReadOnlyList<TreasuryAccountMovementOutputDTO> TreasuryMovements { get; init; } =
        Array.Empty<TreasuryAccountMovementOutputDTO>();

    /// <summary>Number of bank movements in this response page.</summary>
    public int BankMovementCount { get; init; }

    /// <summary>Number of treasury movements in this response page.</summary>
    public int TreasuryMovementCount { get; init; }

    /// <summary>Sum of signed bank statement amounts on this page.</summary>
    public decimal BankAmountTotal { get; init; }

    /// <summary>Sum of positive bank amounts (credits) on this page.</summary>
    public decimal BankCreditTotal { get; init; }

    /// <summary>Absolute sum of negative bank amounts (debits) on this page.</summary>
    public decimal BankDebitTotal { get; init; }

    /// <summary>Sum of treasury inflows on this page.</summary>
    public decimal TreasuryInflowTotal { get; init; }

    /// <summary>Sum of treasury outflows on this page.</summary>
    public decimal TreasuryOutflowTotal { get; init; }

    /// <summary>Treasury inflows minus outflows on this page.</summary>
    public decimal TreasuryNetTotal { get; init; }

    /// <summary>Total unreconciled bank movements matching the filter (all pages).</summary>
    public int BankMovementTotalCount { get; init; }

    /// <summary>Total unreconciled treasury movements matching the filter (all pages).</summary>
    public int TreasuryMovementTotalCount { get; init; }

    /// <summary>Current page (1-based). Reserved for future paging.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Page size used for this query. Reserved for future paging.</summary>
    public int PageSize { get; init; } = 500;
}
