namespace Recruitment.Domain.Entities;

/// <summary>
/// Per-criterion score with mandatory evidence quote when note &gt; 0 (AH-02 / AH-03 / AH-05).
/// </summary>
public sealed class CriterionScoreBreakdown
{
    public required string Code { get; init; }

    public required string Label { get; init; }

    public required decimal Weight { get; init; }

    /// <summary>Partial contribution 0..Weight.</summary>
    public required decimal Note { get; init; }

    public string? Quote { get; init; }

    public int? QuoteOffset { get; init; }

    public bool SemEvidencia { get; init; }

    public bool Conflito { get; init; }

    public IReadOnlyList<string> ConflictQuotes { get; init; } = Array.Empty<string>();

    public string JustificationPt { get; init; } = string.Empty;
}
