namespace Treasury.Application.DTOs;

/// <summary>
/// Compact movement line inside a suggested match combination.
/// </summary>
public record ReconciliationMatchLineDTO
{
    public string Id { get; init; } = string.Empty;

    public string Side { get; init; } = string.Empty;

    public DateOnly Date { get; init; }

    public decimal Amount { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Cheque { get; init; } = string.Empty;
}
