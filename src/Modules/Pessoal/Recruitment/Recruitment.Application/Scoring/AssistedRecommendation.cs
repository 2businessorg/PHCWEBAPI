using Recruitment.Application.DTOs;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Assisted suggestion only. Persisted decision is one of five tokens.
/// selected, rejected, advanced, hired, approved, auto_*, pass and fail are never stored.
/// </summary>
public static class AssistedDecisions
{
    public const string ShortlistSuggest = "shortlist_suggest";
    public const string InterviewSuggest = "interview_suggest";
    public const string WeakFitSuggest = "weak_fit_suggest";
    public const string InsufficientEvidence = "insufficient_evidence";
    public const string ConflictReview = "conflict_review";

    public const string StatusEvidenced = "evidenced";
    public const string StatusNoEvidence = "no_evidence";
    public const string StatusConflict = "conflict";

    public const string WeightSourceRct = "rct";

    public const string DisclaimerPt = HitlCopy.AssistedDisclaimerPt;

    /// <summary>All evidenced and at least this share of RCT weight → shortlist_suggest.</summary>
    public const decimal ShortlistRatio = 0.85m;

    /// <summary>All evidenced and at least this share, below shortlist → interview_suggest.</summary>
    public const decimal InterviewRatio = 0.70m;

    public static bool IsAllowed(string? value) =>
        value is ShortlistSuggest or InterviewSuggest or WeakFitSuggest or InsufficientEvidence or ConflictReview;

    public static string LabelPt(string decision) => decision switch
    {
        ShortlistSuggest => "Sugestão: incluir na shortlist — decisão humana obrigatória",
        InterviewSuggest => "Sugestão: marcar entrevista — decisão humana obrigatória",
        WeakFitSuggest => "Sugestão: ajuste fraco — decisão humana obrigatória",
        InsufficientEvidence => "Sugestão: evidência insuficiente — decisão humana obrigatória",
        ConflictReview => "Sugestão: rever conflito — decisão humana obrigatória",
        _ => throw new InvalidOperationException("BR-08: decision fora do enum assistido.")
    };

    /// <summary>Returns the token only when it is one of the three allowed values.</summary>
    public static string? TryNormalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var token = raw.Trim().ToLowerInvariant();
        if (token.StartsWith("auto_", StringComparison.Ordinal))
            return null;
        return IsAllowed(token) ? token : null;
    }
}

public sealed class AssistedRecommendation
{
    public required string Decision { get; init; }

    public required string LabelPt { get; init; }

    public required string DecisionNote { get; init; }

    public required IReadOnlyList<CriterionAssessmentDto> Criteria { get; init; }

    public required IReadOnlyList<string> GapsPt { get; init; }

    public required IReadOnlyList<CriterionConflictDto> Conflicts { get; init; }
}

public static class AssistedRecommendationBuilder
{
    public static AssistedRecommendation Build(IReadOnlyList<CriterionScoreBreakdown> breakdown)
    {
        var criteria = new List<CriterionAssessmentDto>(breakdown.Count);
        var gaps = new List<string>();
        var conflicts = new List<CriterionConflictDto>();

        foreach (var row in breakdown)
        {
            var status = StatusOf(row);
            IReadOnlyList<string> rowGaps = Array.Empty<string>();
            if (status == AssistedDecisions.StatusNoEvidence)
            {
                var gap = string.IsNullOrWhiteSpace(row.JustificationPt)
                    ? HitlCopy.SemEvidencia
                    : row.JustificationPt.Trim();
                rowGaps = new[] { gap };
                gaps.Add(gap);
            }

            if (status == AssistedDecisions.StatusConflict)
            {
                var quotes = row.ConflictQuotes.Where(q => !string.IsNullOrWhiteSpace(q)).ToArray();
                if (quotes.Length == 0 && !string.IsNullOrWhiteSpace(row.Quote))
                    quotes = new[] { row.Quote! };
                conflicts.Add(new CriterionConflictDto { Code = row.Code, Quotes = quotes });
            }

            criteria.Add(new CriterionAssessmentDto
            {
                Code = row.Code,
                Status = status,
                WeightSource = AssistedDecisions.WeightSourceRct,
                GapsPt = rowGaps
            });
        }

        var decision = Decide(breakdown, criteria);
        var label = AssistedDecisions.LabelPt(decision);
        ForbiddenCopyGuard.ThrowIfForbidden(label, "labelPt");
        ForbiddenCopyGuard.ThrowIfForbidden(AssistedDecisions.DisclaimerPt, "disclaimerPt");

        return new AssistedRecommendation
        {
            Decision = decision,
            LabelPt = label,
            DecisionNote = AssistedDecisions.DisclaimerPt,
            Criteria = criteria,
            GapsPt = gaps,
            Conflicts = conflicts
        };
    }

    public static string StatusOf(CriterionScoreBreakdown row)
    {
        if (row.Conflito)
            return AssistedDecisions.StatusConflict;
        if (row.SemEvidencia || row.Note <= 0 || string.IsNullOrWhiteSpace(row.Quote))
            return AssistedDecisions.StatusNoEvidence;
        return AssistedDecisions.StatusEvidenced;
    }

    private static string Decide(
        IReadOnlyList<CriterionScoreBreakdown> breakdown,
        IReadOnlyList<CriterionAssessmentDto> criteria)
    {
        if (criteria.Count == 0)
            return AssistedDecisions.InsufficientEvidence;
        if (criteria.Any(c => c.Status == AssistedDecisions.StatusConflict))
            return AssistedDecisions.ConflictReview;
        if (criteria.Any(c => c.Status == AssistedDecisions.StatusNoEvidence))
            return AssistedDecisions.InsufficientEvidence;

        var weight = breakdown.Sum(b => b.Weight);
        if (weight <= 0)
            return AssistedDecisions.InsufficientEvidence;

        var ratio = breakdown.Sum(b => b.Note) / weight;
        if (ratio >= AssistedDecisions.ShortlistRatio)
            return AssistedDecisions.ShortlistSuggest;
        if (ratio >= AssistedDecisions.InterviewRatio)
            return AssistedDecisions.InterviewSuggest;
        return AssistedDecisions.WeakFitSuggest;
    }
}
