using System.Text.Json.Serialization;
using Recruitment.Domain.Constants;

namespace Recruitment.Application.DTOs;

public sealed class AnalyzeVacancyResultDto
{
    public required string IdRct { get; init; }

    public required string RctStamp { get; init; }

    public string? RequestedBy { get; init; }

    public int WithCv { get; init; }

    public int Enqueued { get; init; }

    public int Skipped { get; init; }

    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<CandidateEnqueueItemDto> Candidates { get; init; } = Array.Empty<CandidateEnqueueItemDto>();
}

public sealed class CandidateEnqueueItemDto
{
    public required string SrtStamp { get; init; }

    public required string CveStamp { get; init; }

    public bool Enqueued { get; init; }

    public long? OutboxId { get; init; }

    public string Message { get; init; } = string.Empty;

    public int CriteriaCount { get; init; }

    public int IntervenienteCount { get; init; }
}

public sealed class VacancyIaStatusDto
{
    public required string IdRct { get; init; }

    public required string RctStamp { get; init; }

    public int Pendente { get; init; }

    public int Ok { get; init; }

    public int Erro { get; init; }

    public int SemEstado { get; init; }

    public int Total { get; init; }
}

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

    public string PreSelectionNote { get; init; } = HitlCopy.PreSelectionNote;

    /// <summary>Public vacancy id when the caller used /api/recruitment/{idrct}/ranking.</summary>
    public string? IdRct { get; init; }

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

    [JsonPropertyName("totalScore")]
    public decimal TotalScore { get; init; }

    [JsonPropertyName("scoreIsInputNotDecision")]
    public bool ScoreIsInputNotDecision { get; init; } = true;

    [JsonPropertyName("humanDecisionRequired")]
    public bool HumanDecisionRequired { get; init; } = true;

    [JsonPropertyName("disclaimerPt")]
    public string DisclaimerPt { get; init; } = HitlCopy.AssistedDisclaimerPt;

    /// <summary>SRT alias. Never a person name or CV value.</summary>
    [JsonPropertyName("candidateAlias")]
    public string CandidateAlias { get; init; } = string.Empty;

    [JsonPropertyName("labels")]
    public ScorecardLabelsDto Labels { get; init; } = new();

    [JsonPropertyName("recommendation")]
    public AssistedRecommendationDto? Recommendation { get; init; }

    [JsonPropertyName("readiness")]
    public ScorecardReadinessDto? Readiness { get; init; }

    [JsonPropertyName("strengthsPt")]
    public IReadOnlyList<string> StrengthsPt { get; init; } = Array.Empty<string>();

    [JsonPropertyName("interviewValidationQuestionPt")]
    public string? InterviewValidationQuestionPt { get; init; }

    [JsonPropertyName("criteria")]
    public IReadOnlyList<CriterionAssessmentDto> Criteria { get; init; } = Array.Empty<CriterionAssessmentDto>();

    [JsonPropertyName("gapsPt")]
    public IReadOnlyList<string> GapsPt { get; init; } = Array.Empty<string>();

    [JsonPropertyName("conflicts")]
    public IReadOnlyList<CriterionConflictDto> Conflicts { get; init; } = Array.Empty<CriterionConflictDto>();

    [JsonPropertyName("breakdown")]
    public IReadOnlyList<CriterionBreakdownDto> Breakdown { get; init; } = Array.Empty<CriterionBreakdownDto>();
}

/// <summary>Assisted suggestion. decision is only avancar, em_duvida, or nao_avancar.</summary>
public sealed class AssistedRecommendationDto
{
    [JsonPropertyName("decision")]
    public required string Decision { get; init; }

    [JsonPropertyName("labelPt")]
    public required string LabelPt { get; init; }

    [JsonPropertyName("rationalePt")]
    public string? RationalePt { get; init; }

    [JsonPropertyName("decision_note")]
    public required string DecisionNote { get; init; }
}

public sealed class ScorecardLabelsDto
{
    [JsonPropertyName("humanDecisionRequired")]
    public bool HumanDecisionRequired { get; init; } = true;

    [JsonPropertyName("criteriaWithEvidence")]
    public string CriteriaWithEvidence { get; init; } = "0/0";

    [JsonPropertyName("piiExcludedFromScore")]
    public bool PiiExcludedFromScore { get; init; }
}

public sealed class ScorecardReadinessDto
{
    [JsonPropertyName("ocrQualityPct")]
    public decimal? OcrQualityPct { get; init; }

    [JsonPropertyName("ocrEngine")]
    public string OcrEngine { get; init; } = string.Empty;

    [JsonPropertyName("criteriaCovered")]
    public string CriteriaCovered { get; init; } = "0/0";

    [JsonPropertyName("dataAlert")]
    public ScorecardDataAlertDto DataAlert { get; init; } = new();

    [JsonPropertyName("piiTypesExcluded")]
    public IReadOnlyList<string> PiiTypesExcluded { get; init; } = Array.Empty<string>();
}

public sealed class ScorecardDataAlertDto
{
    /// <summary>ok | warn | critical</summary>
    [JsonPropertyName("level")]
    public string Level { get; init; } = "ok";

    [JsonPropertyName("messagePt")]
    public string MessagePt { get; init; } = string.Empty;
}

public sealed class CriterionAssessmentDto
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    /// <summary>evidenced | no_evidence | conflict</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("weightSource")]
    public string WeightSource { get; init; } = "rct";

    [JsonPropertyName("weightPct")]
    public decimal WeightPct { get; init; }

    [JsonPropertyName("required")]
    public bool? Required { get; init; }

    [JsonPropertyName("note")]
    public decimal Note { get; init; }

    [JsonPropertyName("maxWeight")]
    public decimal MaxWeight { get; init; }

    [JsonPropertyName("barPct")]
    public decimal BarPct { get; init; }

    [JsonPropertyName("quote")]
    public string? Quote { get; init; }

    [JsonPropertyName("justificationPt")]
    public string JustificationPt { get; init; } = string.Empty;

    [JsonPropertyName("semEvidencia")]
    public bool SemEvidencia { get; init; }

    [JsonPropertyName("conflito")]
    public bool Conflito { get; init; }

    [JsonPropertyName("gapsPt")]
    public IReadOnlyList<string> GapsPt { get; init; } = Array.Empty<string>();
}

public sealed class CriterionConflictDto
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("quotes")]
    public IReadOnlyList<string> Quotes { get; init; } = Array.Empty<string>();
}
