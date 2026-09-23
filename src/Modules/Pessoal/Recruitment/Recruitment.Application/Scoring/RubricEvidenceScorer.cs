using System.Globalization;
using System.Text;
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

    /// <summary>
    /// Resolves a model quote to a contiguous slice of <paramref name="uTexto"/>.
    /// Exact match first, then a deterministic whitespace/Unicode fold
    /// (NFKC, NBSP and thin spaces to space, zero-width dropped, whitespace collapsed).
    /// The returned slice is always an ordinal subset of the original text.
    /// </summary>
    public static bool TryResolveQuote(string uTexto, string? quote, out string? resolved)
    {
        resolved = null;
        if (string.IsNullOrWhiteSpace(quote) || string.IsNullOrEmpty(uTexto))
            return false;

        var exact = uTexto.IndexOf(quote, StringComparison.Ordinal);
        if (exact >= 0)
        {
            resolved = uTexto.Substring(exact, quote.Length);
            return true;
        }

        var ignoreCase = uTexto.IndexOf(quote, StringComparison.OrdinalIgnoreCase);
        if (ignoreCase >= 0)
        {
            resolved = uTexto.Substring(ignoreCase, quote.Length);
            return true;
        }

        var (normEvidence, map) = NormalizeWithMap(uTexto);
        var normQuote = NormalizeQuote(quote);
        if (normQuote.Length == 0 || map.Length == 0)
            return false;

        var idx = normEvidence.IndexOf(normQuote, StringComparison.Ordinal);
        if (idx < 0)
            idx = normEvidence.IndexOf(normQuote, StringComparison.OrdinalIgnoreCase);
        if (idx < 0 || idx + normQuote.Length > map.Length)
            return false;

        var start = map[idx];
        var end = map[idx + normQuote.Length - 1] + 1;
        if (start < 0 || end > uTexto.Length || end <= start)
            return false;

        resolved = uTexto[start..end];
        return uTexto.IndexOf(resolved, StringComparison.Ordinal) >= 0;
    }

    private static string NormalizeQuote(string quote) => NormalizeWithMap(quote).Normalized;

    private static (string Normalized, int[] Map) NormalizeWithMap(string text)
    {
        var chars = new StringBuilder(text.Length);
        var map = new List<int>(text.Length);
        var previousWasSpace = false;

        for (var i = 0; i < text.Length; i++)
        {
            var piece = text[i].ToString().Normalize(NormalizationForm.FormKC);
            foreach (var n in piece)
            {
                if (IsDroppedFormatChar(n))
                    continue;

                var ch = IsFoldableSpace(n) ? ' ' : n;
                if (ch == ' ')
                {
                    if (previousWasSpace)
                        continue;
                    previousWasSpace = true;
                }
                else
                {
                    previousWasSpace = false;
                }

                chars.Append(ch);
                map.Add(i);
            }
        }

        return (chars.ToString(), map.ToArray());
    }

    private static bool IsDroppedFormatChar(char c) =>
        c is '\u00AD' or '\u200B' or '\u200C' or '\u200D' or '\uFEFF';

    private static bool IsFoldableSpace(char c) =>
        c is ' ' or '\t' or '\n' or '\r' or '\u00A0' or '\u2000' or '\u2001' or '\u2002'
            or '\u2003' or '\u2004' or '\u2005' or '\u2006' or '\u2007' or '\u2008' or '\u2009'
            or '\u200A' or '\u202F' or '\u205F' or '\u3000'
        || char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;

    private sealed record EvidenceMatch(string Hint, string Excerpt, int Offset);
}

public interface IRubricEvidenceScorer
{
    AnalysisScoreResult Score(string? cvText, IReadOnlyList<RctCriterion> criteria);
}
