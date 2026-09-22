namespace Recruitment.Domain.Entities;

/// <summary>
/// One usable characteristic (crt) from an RCT with its native weight (AH-01 / BR-01 / BR-11).
/// Weights must come from RCT data — never hardcode production weights in the scorer.
/// </summary>
public sealed class RctCriterion
{
    public required string Code { get; init; }

    public required string Label { get; init; }

    /// <summary>Weight contribution toward 0–100 (RCT native).</summary>
    public required decimal Weight { get; init; }

    /// <summary>
    /// Lexical hints used only to locate evidence in u_texto.
    /// Hints are stored with the RCT criterion row; the scorer does not invent criteria.
    /// </summary>
    public IReadOnlyList<string> EvidenceHints { get; init; } = Array.Empty<string>();
}
