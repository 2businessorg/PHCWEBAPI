namespace Treasury.Application.DTOs;

/// <summary>
/// Simplified treasury account movement (PHC <c>ba</c>) for API and AI consumers.
/// Inflow and outflow are kept as separate PHC fields.
/// </summary>
public record TreasuryAccountMovementOutputDTO
{
    /// <summary>Movement stamp.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Document date (yyyy-MM-dd).</summary>
    public DateOnly Date { get; init; }

    /// <summary>Value date (yyyy-MM-dd).</summary>
    public DateOnly ValueDate { get; init; }

    /// <summary>Document reference.</summary>
    public string Document { get; init; } = string.Empty;

    /// <summary>Movement description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Inflow amount (<c>entrada</c>).</summary>
    public decimal Inflow { get; init; }

    /// <summary>Outflow amount (<c>saida</c>).</summary>
    public decimal Outflow { get; init; }

    /// <summary>Cheque / reference.</summary>
    public string Cheque { get; init; } = string.Empty;
}
