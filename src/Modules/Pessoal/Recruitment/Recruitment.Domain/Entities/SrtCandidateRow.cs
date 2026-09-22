namespace Recruitment.Domain.Entities;

/// <summary>
/// SRT row projection for HITL ranking. Selection/apurado/entrevista fields are read-only here.
/// </summary>
public sealed class SrtCandidateRow
{
    public required string SrtStamp { get; init; }

    public required string CveStamp { get; init; }

    public required string RctStamp { get; init; }

    public string? CandidateName { get; init; }

    public decimal? ScoreIa { get; init; }

    public string? JustificationJson { get; init; }

    public string? EstadoIa { get; init; }

    /// <summary>Native PHC field — IA must never write (BR-04).</summary>
    public string? Condp { get; init; }

    public string? ModeloIa { get; init; }

    public string? PromptVerIa { get; init; }

    public DateTime? StampIa { get; init; }

    /// <summary>
    /// Snapshot of selection-related native fields used in tests to prove BR-03 zero side-effects.
    /// </summary>
    public string? SelectionStateSnapshot { get; init; }
}
