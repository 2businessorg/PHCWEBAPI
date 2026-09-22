namespace Treasury.Application.DTOs;

/// <summary>
/// Suggested pairing of bank (BR) and treasury (BA) movements.
/// </summary>
public record ReconciliationMatchCombinationDTO
{
    public string Status { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;

    public int BankCount { get; init; }

    public int TreasuryCount { get; init; }

    public decimal BankAmountTotal { get; init; }

    public decimal TreasuryAmountTotal { get; init; }

    public int? DateGapDays { get; init; }

    public IReadOnlyList<ReconciliationMatchLineDTO> Bank { get; init; } =
        Array.Empty<ReconciliationMatchLineDTO>();

    public IReadOnlyList<ReconciliationMatchLineDTO> Treasury { get; init; } =
        Array.Empty<ReconciliationMatchLineDTO>();
}
