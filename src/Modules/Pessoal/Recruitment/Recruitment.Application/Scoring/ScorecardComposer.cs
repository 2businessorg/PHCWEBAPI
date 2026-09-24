using System.Globalization;
using Recruitment.Application.DTOs;
using Recruitment.Application.Privacy;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;
using Shared.Abstractions.DocumentTextExtraction;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Report-ready scorecard stored in srt.u_justia. Does not render HTML.
/// Weights always come from the RCT breakdown already scored. No selection/condp.
/// </summary>
public static class ScorecardComposer
{
    public static string CandidateAlias(string srtStamp)
    {
        var stamp = (srtStamp ?? string.Empty).Trim();
        var n = 0;
        foreach (var ch in stamp)
            n = (n * 31 + ch) % 1000;
        return string.Create(CultureInfo.InvariantCulture, $"Candidato {n:000} · perfil pseudonimizado");
    }

    public static JustificationPayloadDto Compose(
        AnalysisScoreResult score,
        string srtStamp,
        DocumentTextExtractionResult ocr,
        CloudEgressPreparation egress)
    {
        var assisted = AssistedRecommendationBuilder.Build(score.Breakdown);
        var rows = BuildCriteria(score.Breakdown);
        var evidenced = rows.Count(r => r.Status == AssistedDecisions.StatusEvidenced);
        var covered = $"{evidenced}/{rows.Count}";
        var piiTypes = PiiTypes(egress);
        var alert = DataAlert(rows);

        var cloud = score.UsedLlm;
        return new JustificationPayloadDto
        {
            Engine = score.EngineName,
            PromptVer = score.PromptVer,
            StampUtc = score.StampUtc,
            UsedLlm = score.UsedLlm,
            Total = score.TotalScore,
            TotalScore = score.TotalScore,
            ScoreIsInputNotDecision = true,
            HumanDecisionRequired = true,
            DisclaimerPt = AssistedDecisions.DisclaimerPt,
            CandidateAlias = CandidateAlias(srtStamp),
            Labels = new ScorecardLabelsDto
            {
                HumanDecisionRequired = true,
                CriteriaWithEvidence = covered,
                PiiExcludedFromScore = cloud && egress.EgressAllowed
            },
            Recommendation = new AssistedRecommendationDto
            {
                Decision = assisted.Decision,
                LabelPt = assisted.LabelPt,
                RationalePt = cloud ? score.RationalePt : null,
                DecisionNote = assisted.DecisionNote
            },
            Readiness = new ScorecardReadinessDto
            {
                OcrQualityPct = null,
                OcrEngine = OcrEngineLabel(ocr),
                CriteriaCovered = covered,
                DataAlert = alert,
                PiiTypesExcluded = piiTypes
            },
            StrengthsPt = cloud ? score.StrengthsPt : Array.Empty<string>(),
            InterviewValidationQuestionPt = cloud ? score.InterviewValidationQuestionPt : null,
            Criteria = rows,
            GapsPt = assisted.GapsPt,
            Conflicts = assisted.Conflicts,
            Breakdown = score.Breakdown.Select(b => new CriterionBreakdownDto
            {
                Code = b.Code,
                Label = b.Label,
                Weight = b.Weight,
                Note = b.Note,
                Quote = b.Quote,
                QuoteOffset = b.QuoteOffset,
                SemEvidencia = b.SemEvidencia,
                Conflito = b.Conflito,
                ConflictQuotes = b.ConflictQuotes,
                JustificationPt = b.JustificationPt
            }).ToArray()
        };
    }

    public static IReadOnlyList<CriterionAssessmentDto> BuildCriteria(IReadOnlyList<CriterionScoreBreakdown> breakdown)
    {
        var weightSum = breakdown.Sum(b => b.Weight);
        var rows = new List<CriterionAssessmentDto>(breakdown.Count);
        foreach (var row in breakdown)
        {
            var status = AssistedRecommendationBuilder.StatusOf(row);
            var max = row.Weight;
            var bar = max <= 0 ? 0m : decimal.Round(row.Note / max * 100m, 1, MidpointRounding.AwayFromZero);
            var weightPct = weightSum <= 0
                ? 0m
                : decimal.Round(row.Weight / weightSum * 100m, 1, MidpointRounding.AwayFromZero);
            IReadOnlyList<string> gaps = status == AssistedDecisions.StatusNoEvidence
                ? new[] { string.IsNullOrWhiteSpace(row.JustificationPt) ? HitlCopy.SemEvidencia : row.JustificationPt }
                : Array.Empty<string>();

            rows.Add(new CriterionAssessmentDto
            {
                Code = row.Code,
                Label = row.Label,
                Status = status,
                WeightSource = AssistedDecisions.WeightSourceRct,
                WeightPct = weightPct,
                Required = null,
                Note = row.Note,
                MaxWeight = max,
                BarPct = bar,
                Quote = row.Quote,
                JustificationPt = row.JustificationPt,
                SemEvidencia = row.SemEvidencia,
                Conflito = row.Conflito,
                GapsPt = gaps
            });
        }

        return rows;
    }

    private static ScorecardDataAlertDto DataAlert(IReadOnlyList<CriterionAssessmentDto> rows)
    {
        var evidenced = rows.Count(r => r.Status == AssistedDecisions.StatusEvidenced);
        if (rows.Count == 0 || evidenced == 0)
        {
            return new ScorecardDataAlertDto
            {
                Level = "critical",
                MessagePt = "Cobertura de critérios insuficiente."
            };
        }

        if (rows.Any(r => r.Status != AssistedDecisions.StatusEvidenced))
        {
            return new ScorecardDataAlertDto
            {
                Level = "warn",
                MessagePt = "Há critérios sem evidência ou em conflito."
            };
        }

        return new ScorecardDataAlertDto
        {
            Level = "ok",
            MessagePt = "Dados suficientes para a sugestão assistida."
        };
    }

    private static IReadOnlyList<string> PiiTypes(CloudEgressPreparation egress)
    {
        var entities = egress.Result?.Entities;
        if (entities is null || entities.Count == 0)
            return Array.Empty<string>();

        return entities
            .Select(e => string.IsNullOrWhiteSpace(e.EntityType) ? "UNKNOWN" : e.EntityType)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToArray();
    }

    private static string OcrEngineLabel(DocumentTextExtractionResult ocr)
    {
        var engine = string.IsNullOrWhiteSpace(ocr.EngineName) ? "unknown" : ocr.EngineName.Trim();
        return ocr.Source + "/" + engine;
    }
}
