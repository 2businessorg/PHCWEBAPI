using System.Text.Json.Serialization;
using Recruitment.Domain.Constants;

namespace Recruitment.Application.DTOs;

public sealed class EnqueueAnalysisResultDto
{
    public bool Enqueued { get; init; }

    public long? OutboxId { get; init; }

    public string Message { get; init; } = string.Empty;

    public int CriteriaCount { get; init; }

    public int IntervenienteCount { get; init; }
}

public sealed class RctRankingDto
{
    public string Title { get; init; } = HitlCopy.RankingTitle;

    public string Footer { get; init; } = HitlCopy.Footer;

    public required string RctStamp { get; init; }

    public required IReadOnlyList<RankedCandidateDto> Candidates { get; init; }
}

public sealed class RankedCandidateDto
{
    public int Position { get; init; }

    public required string SrtStamp { get; init; }

    public required string CveStamp { get; init; }

    public string? CandidateName { get; init; }

    public decimal? ScoreTotal { get; init; }

    public string? EstadoIa { get; init; }

    /// <summary>Native condp — read-only (BR-04).</summary>
    public string? Condp { get; init; }

    public string? Modelo { get; init; }

    public string? PromptVer { get; init; }

    public DateTime? StampIa { get; init; }

    public IReadOnlyList<CriterionBreakdownDto> Breakdown { get; init; } = Array.Empty<CriterionBreakdownDto>();
}

public sealed class CriterionBreakdownDto
{
    public required string Code { get; init; }

    public required string Label { get; init; }

    public decimal Weight { get; init; }

    public decimal Note { get; init; }

    public string? Quote { get; init; }

    public int? QuoteOffset { get; init; }

    public bool SemEvidencia { get; init; }

    public bool Conflito { get; init; }

    public IReadOnlyList<string> ConflictQuotes { get; init; } = Array.Empty<string>();

    public string JustificationPt { get; init; } = string.Empty;
}

public sealed class CandidateComparisonDto
{
    public string Title { get; init; } = HitlCopy.RankingTitle;

    public string Footer { get; init; } = HitlCopy.Footer;

    public required RankedCandidateDto First { get; init; }

    public required RankedCandidateDto Other { get; init; }

    public required IReadOnlyList<CriterionDiffDto> TopDiffs { get; init; }
}

public sealed class CriterionDiffDto
{
    public required string Code { get; init; }

    public required string Label { get; init; }

    public decimal FirstNote { get; init; }

    public decimal OtherNote { get; init; }

    public string? FirstQuote { get; init; }

    public string? OtherQuote { get; init; }
}

/// <summary>JSON shape stored in srt.u_justia.</summary>
public sealed class JustificationPayloadDto
{
    [JsonPropertyName("engine")]
    public string Engine { get; init; } = string.Empty;

    [JsonPropertyName("prompt_ver")]
    public string PromptVer { get; init; } = string.Empty;

    [JsonPropertyName("stamp_utc")]
    public DateTime StampUtc { get; init; }

    [JsonPropertyName("used_llm")]
    public bool UsedLlm { get; init; }

    [JsonPropertyName("total")]
    public decimal Total { get; init; }

    [JsonPropertyName("breakdown")]
    public IReadOnlyList<CriterionBreakdownDto> Breakdown { get; init; } = Array.Empty<CriterionBreakdownDto>();
}
