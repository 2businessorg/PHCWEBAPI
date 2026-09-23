using System.Globalization;
using System.Text;
using System.Text.Json;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Scores pseudonymized CV text with Qwen (OpenAI-compatible ILocalChatModel).
/// Quotes must be contiguous subsets of the text passed in (AH-02). No rubric fallback.
/// </summary>
public sealed class QwenCloudScoreEngine : ICandidateScoreEngine
{
    public const string PromptVer = "qwen-cloud-v2";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IRecruitmentLlmClient _llm;

    public QwenCloudScoreEngine(IRecruitmentLlmClient llm)
    {
        _llm = llm;
    }

    public async Task<AnalysisScoreResult> ScoreAsync(
        string evidenceText,
        IReadOnlyList<RctCriterion> criteria,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        if (criteria.Count == 0)
            throw new InvalidOperationException("AH-01/BR-01: sem crt utilizaveis — score proibido.");

        if (string.IsNullOrWhiteSpace(_llm.ModelName))
            throw new InvalidOperationException("BR-08: modelo Qwen nao configurado.");

        var content = await _llm.CompleteAsync(SystemPrompt, BuildUserPrompt(evidenceText, criteria), cancellationToken);
        var parsed = Parse(content);
        var byCode = new Dictionary<string, CloudCriterionRow>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in parsed)
        {
            if (!string.IsNullOrWhiteSpace(row.Code))
                byCode.TryAdd(row.Code.Trim(), row);
        }

        var breakdown = new List<CriterionScoreBreakdown>(criteria.Count);
        foreach (var crt in criteria)
        {
            byCode.TryGetValue(crt.Code, out var row);
            breakdown.Add(MapCriterion(evidenceText, crt, row));
        }

        var total = breakdown.Sum(b => b.Note);
        if (total < 0) total = 0;
        if (total > 100) total = 100;

        var assisted = AssistedRecommendationBuilder.Build(breakdown);

        return new AnalysisScoreResult
        {
            TotalScore = decimal.Round(total, 2, MidpointRounding.AwayFromZero),
            Breakdown = breakdown,
            EngineName = _llm.ModelName,
            PromptVer = PromptVer,
            StampUtc = DateTime.UtcNow,
            UsedLlm = true,
            AssistedDecision = assisted.Decision
        };
    }

    internal const string SystemPrompt =
        "Es um avaliador de recrutamento. Devolves apenas JSON, sem markdown. " +
        "Prompt " + PromptVer + ". " +
        "Pontua somente os criterios recebidos. note e um numero entre 0 e weight. " +
        "Se note>0, quote e uma citacao contigua exacta do texto (maximo 240 caracteres). " +
        "Se nao ha evidencia, note=0, quote vazio, justificationPt=\"sem evidencia no CV\". " +
        "Justificacao em pt-PT. Nao inventes factos fora do texto. " +
        "O score e input ao RH. A sugestao assistida usa so decision avancar, em_duvida ou nao_avancar. " +
        "Sem evidencia ou conflito no criterio: a sugestao e em_duvida. " +
        "Formato: {\"criteria\":[{\"code\":\"\",\"note\":0,\"quote\":\"\",\"justificationPt\":\"\",\"conflito\":false}]," +
        "\"recommendation\":{\"decision\":\"em_duvida\"}}";

    internal static string BuildUserPrompt(string evidenceText, IReadOnlyList<RctCriterion> criteria)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Criterios (code | label | weight | hints):");
        foreach (var crt in criteria)
        {
            var hints = crt.EvidenceHints.Count == 0
                ? "-"
                : string.Join(", ", crt.EvidenceHints);
            sb.Append(crt.Code).Append(" | ").Append(crt.Label).Append(" | ")
                .Append(crt.Weight.ToString(CultureInfo.InvariantCulture))
                .Append(" | ").AppendLine(hints);
        }

        sb.AppendLine();
        sb.AppendLine("Texto ja pseudonimizado (unica fonte de citacoes):");
        sb.Append(evidenceText);
        return sb.ToString();
    }

    private static IReadOnlyList<CloudCriterionRow> Parse(string? content)
    {
        var json = ExtractJson(content);
        CloudScoreDocument? doc;
        try
        {
            doc = JsonSerializer.Deserialize<CloudScoreDocument>(json, JsonOptions);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("BR-08: resposta Qwen nao e JSON de score.");
        }

        return doc?.Criteria ?? new List<CloudCriterionRow>();
    }

    /// <summary>
    /// Model decision is kept only when it is avancar, em_duvida, or nao_avancar.
    /// Any other token is dropped and is not copied into the score.
    /// </summary>
    public static string? ReadAllowedDecision(string? content)
    {
        var json = ExtractJson(content);
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("recommendation", out var recommendation)
                && !doc.RootElement.TryGetProperty("Recommendation", out recommendation))
                return null;

            if (recommendation.ValueKind != JsonValueKind.Object)
                return null;

            if (!recommendation.TryGetProperty("decision", out var decision)
                && !recommendation.TryGetProperty("Decision", out decision))
                return null;

            return AssistedDecisions.TryNormalize(decision.GetString());
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static string ExtractJson(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("BR-08: resposta Qwen vazia.");

        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNl = trimmed.IndexOf('\n');
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNl >= 0 && lastFence > firstNl)
                trimmed = trimmed[(firstNl + 1)..lastFence].Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start < 0 || end <= start)
            throw new InvalidOperationException("BR-08: resposta Qwen nao e JSON de score.");

        return trimmed[start..(end + 1)];
    }

    private static CriterionScoreBreakdown MapCriterion(
        string evidenceText,
        RctCriterion crt,
        CloudCriterionRow? row)
    {
        var note = row?.Note ?? 0m;
        if (note < 0) note = 0;
        if (note > crt.Weight) note = crt.Weight;
        note = decimal.Round(note, 2, MidpointRounding.AwayFromZero);

        var quote = string.IsNullOrWhiteSpace(row?.Quote) ? null : row!.Quote!.Trim();
        if (quote is not null && quote.Length > RubricEvidenceScorer.MaxQuoteLength)
            quote = quote[..RubricEvidenceScorer.MaxQuoteLength];

        if (note > 0)
        {
            if (string.IsNullOrEmpty(quote))
                throw new InvalidOperationException($"AH-02: criterio {crt.Code} com nota sem citacao.");

            RubricEvidenceScorer.AssertQuoteIsSubset(evidenceText, quote);
        }
        else
        {
            quote = null;
        }

        var justification = string.IsNullOrWhiteSpace(row?.JustificationPt)
            ? (note <= 0 ? HitlCopy.SemEvidencia : $"Evidencia encontrada para '{crt.Label}'.")
            : row!.JustificationPt!.Trim();
        ForbiddenCopyGuard.ThrowIfForbidden(justification, crt.Code);

        var offset = quote is null ? (int?)null : evidenceText.IndexOf(quote, StringComparison.Ordinal);
        if (offset < 0 && quote is not null)
            offset = evidenceText.IndexOf(quote, StringComparison.OrdinalIgnoreCase);

        return new CriterionScoreBreakdown
        {
            Code = crt.Code,
            Label = crt.Label,
            Weight = crt.Weight,
            Note = note,
            Quote = quote,
            QuoteOffset = note > 0 ? offset : null,
            SemEvidencia = note <= 0,
            Conflito = row?.Conflito == true && note > 0,
            JustificationPt = note <= 0 ? HitlCopy.SemEvidencia : justification
        };
    }

    private sealed class CloudScoreDocument
    {
        public List<CloudCriterionRow>? Criteria { get; set; }
    }

    private sealed class CloudCriterionRow
    {
        public string? Code { get; set; }

        public decimal Note { get; set; }

        public string? Quote { get; set; }

        public string? JustificationPt { get; set; }

        public bool Conflito { get; set; }
    }
}
