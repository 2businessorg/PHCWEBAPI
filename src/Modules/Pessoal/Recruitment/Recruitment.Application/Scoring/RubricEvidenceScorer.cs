using System.Globalization;
using System.Text.RegularExpressions;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Deterministic rubric + evidence scorer (ADOPT research pattern).
/// AH-01: only RCT criteria. AH-02/03: quote required for note&gt;0. AH-05: conflict flag.
/// AH-06: pt-PT justification. Semantic-only / embeddings-as-sole-engine REJECTED.
/// </summary>
public sealed class RubricEvidenceScorer : IRubricEvidenceScorer
{
    public const int MaxQuoteLength = 240;
    public const string EngineName = "rubric-evidence-v1";
    public const string PromptVer = "deterministic-v1";

    private static readonly Regex YearsRegex = new(
        @"(\d{1,2})\s*(?:\+)?\s*(?:anos?|years?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public AnalysisScoreResult Score(string? cvText, IReadOnlyList<RctCriterion> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        if (criteria.Count == 0)
            throw new InvalidOperationException("AH-01/BR-01: sem crt utilizaveis — score proibido.");

        var text = cvText ?? string.Empty;
        var stamp = DateTime.UtcNow;
        var breakdown = new List<CriterionScoreBreakdown>(criteria.Count);

        foreach (var crt in criteria)
        {
            breakdown.Add(ScoreCriterion(text, crt));
        }

        var total = breakdown.Sum(b => b.Note);
        if (total < 0) total = 0;
        if (total > 100) total = 100;

        return new AnalysisScoreResult
        {
            TotalScore = decimal.Round(total, 2, MidpointRounding.AwayFromZero),
            Breakdown = breakdown,
            EngineName = EngineName,
            PromptVer = PromptVer,
            StampUtc = stamp,
            UsedLlm = false
        };
    }

    private static CriterionScoreBreakdown ScoreCriterion(string text, RctCriterion crt)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return NoEvidence(crt, "Texto do CV vazio.");
        }

        var matches = FindEvidenceExcerpts(text, crt.EvidenceHints);
        if (matches.Count == 0)
            return NoEvidence(crt, HitlCopy.SemEvidencia);

        var conflict = DetectConflict(matches, text);
        var primary = matches[0];
        var quote = TruncateQuote(primary.Excerpt);
        AssertQuoteIsSubset(text, quote);

        // Conflict: flag both excerpts; do not silent-average — use lower band (AH-05).
        var note = conflict
            ? decimal.Round(crt.Weight * 0.4m, 2, MidpointRounding.AwayFromZero)
            : EstimateNote(crt, text, matches);

        if (note > 0 && string.IsNullOrWhiteSpace(quote))
            note = 0;

        var justification = conflict
            ? $"Conflito detetado no criterio '{crt.Label}'. RH deve rever as citacoes."
            : $"Evidencia encontrada para '{crt.Label}'.";

        ForbiddenCopyGuard.ThrowIfForbidden(justification, nameof(justification));

        return new CriterionScoreBreakdown
        {
            Code = crt.Code,
            Label = crt.Label,
            Weight = crt.Weight,
            Note = note,
            Quote = note > 0 ? quote : null,
            QuoteOffset = note > 0 ? primary.Offset : null,
            SemEvidencia = note <= 0,
            Conflito = conflict,
            ConflictQuotes = conflict
                ? matches.Take(2).Select(m => TruncateQuote(m.Excerpt)).ToArray()
                : Array.Empty<string>(),
            JustificationPt = note <= 0 ? HitlCopy.SemEvidencia : justification
        };
    }

    private static CriterionScoreBreakdown NoEvidence(RctCriterion crt, string reason) => new()
    {
        Code = crt.Code,
        Label = crt.Label,
        Weight = crt.Weight,
        Note = 0,
        Quote = null,
        QuoteOffset = null,
        SemEvidencia = true,
        Conflito = false,
        JustificationPt = reason
    };

    private static decimal EstimateNote(
        RctCriterion crt,
        string text,
        IReadOnlyList<EvidenceMatch> matches)
    {
        // Strength scales with distinct hint hits; capped at criterion weight (AH-01).
        var distinctHits = matches.Select(m => m.Hint.ToLowerInvariant()).Distinct().Count();
        var ratio = distinctHits switch
        {
            1 => 0.40m,
            2 => 0.72m,
            _ => 1.00m
        };

        // Mild boost when years mentioned near first hit (still requires citation).
        if (YearsRegex.IsMatch(matches[0].Excerpt) || YearsRegex.IsMatch(text))
            ratio = Math.Min(1.00m, ratio + 0.08m);

        return decimal.Round(crt.Weight * ratio, 2, MidpointRounding.AwayFromZero);
    }

    private static bool DetectConflict(IReadOnlyList<EvidenceMatch> matches, string fullText)
    {
        var yearValues = new List<int>();

        void Collect(string s)
        {
            foreach (Match rm in YearsRegex.Matches(s))
            {
                if (int.TryParse(rm.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var y))
                    yearValues.Add(y);
            }
        }

        foreach (var m in matches)
            Collect(m.Excerpt);
        Collect(fullText);

        if (yearValues.Count < 2)
            return false;

        var min = yearValues.Min();
        var max = yearValues.Max();
        // e.g. "3 anos" and "10 anos" on same criterion → conflict (AH-05)
        return max - min >= 5;
    }

    private static List<EvidenceMatch> FindEvidenceExcerpts(string text, IReadOnlyList<string> hints)
    {
        var results = new List<EvidenceMatch>();
        if (hints.Count == 0)
            return results;

        foreach (var hint in hints.Where(h => !string.IsNullOrWhiteSpace(h)))
        {
            var idx = text.IndexOf(hint, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                continue;

            var start = Math.Max(0, idx - 40);
            var len = Math.Min(MaxQuoteLength, text.Length - start);
            var excerpt = text.Substring(start, len).Trim();
            results.Add(new EvidenceMatch(hint, excerpt, start));
        }

        return results;
    }

    internal static string TruncateQuote(string excerpt)
    {
        if (excerpt.Length <= MaxQuoteLength)
            return excerpt;
        return excerpt[..MaxQuoteLength];
    }

    /// <summary>AH-02 falsifier: quote must be a contiguous subset of u_texto.</summary>
    public static void AssertQuoteIsSubset(string uTexto, string? quote)
    {
        if (string.IsNullOrEmpty(quote))
            return;

        if (uTexto.IndexOf(quote, StringComparison.Ordinal) < 0
            && uTexto.IndexOf(quote, StringComparison.OrdinalIgnoreCase) < 0)
        {
            throw new InvalidOperationException(
                "AH-02: citacao nao pertence a u_texto (falsificador RH / DTTest).");
        }
    }

    private sealed record EvidenceMatch(string Hint, string Excerpt, int Offset);
}

public interface IRubricEvidenceScorer
{
    AnalysisScoreResult Score(string? cvText, IReadOnlyList<RctCriterion> criteria);
}
