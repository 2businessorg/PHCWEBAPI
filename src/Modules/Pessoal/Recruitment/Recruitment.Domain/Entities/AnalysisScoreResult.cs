namespace Recruitment.Domain.Entities;

/// <summary>
/// Deterministic rubric result persisted on SRT (BR-02), never as canonical CVE multi-RCT score.
/// </summary>
public sealed class AnalysisScoreResult
{
    public required decimal TotalScore { get; init; }

    public required IReadOnlyList<CriterionScoreBreakdown> Breakdown { get; init; }

    public required string EngineName { get; init; }

    public required string PromptVer { get; init; }

    public required DateTime StampUtc { get; init; }

    public bool UsedLlm { get; init; }
}
