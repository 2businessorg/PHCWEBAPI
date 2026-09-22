namespace Treasury.Application.DTOs;

/// <summary>
/// Simplified imported bank movement (PHC <c>br</c>) for API and AI consumers.
/// </summary>
public record ImportedBankMovementOutputDTO
{
    /// <summary>Movement stamp.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Document date (yyyy-MM-dd).</summary>
    public DateOnly Date { get; init; }

    /// <summary>Value date (yyyy-MM-dd).</summary>
    public DateOnly ValueDate { get; init; }

    /// <summary>Bank description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Signed statement amount.</summary>
    public decimal Amount { get; init; }

    /// <summary>Document reference.</summary>
    public string Document { get; init; } = string.Empty;

    /// <summary>Cheque / reference.</summary>
    public string Cheque { get; init; } = string.Empty;
}
