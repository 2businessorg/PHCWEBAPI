namespace Treasury.Application.DTOs;

/// <summary>
/// Inclusive date window used by a reconciliation query.
/// </summary>
public record ReconciliationPeriodOutputDTO
{
    /// <summary>Start date (inclusive).</summary>
    public DateOnly From { get; init; }

    /// <summary>End date (inclusive).</summary>
    public DateOnly To { get; init; }
}
