using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Scores pseudonymized CV text with Qwen (OpenAI-compatible ILocalChatModel).
/// Quotes must be contiguous subsets of the text passed in (AH-02). No rubric fallback.
/// </summary>
public sealed class QwenCloudScoreEngine : ICandidateScoreEngine
{
    public const string PromptVer = "qwen-cloud-v5";

    public const string RejectedQuoteJustification = "AH-02: citacao rejeitada; criterio sem nota.";

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

        var userPrompt = BuildUserPrompt(evidenceText, criteria);
        var content = await _llm.CompleteAsync(SystemPrompt, userPrompt, cancellationToken);
        if (TryMaterialize(content, evidenceText, criteria, out var result, out var parseError))
            return result!;

        content = await _llm.CompleteAsync(
            RepairSystemPrompt,
            RepairUserPrefix + userPrompt,
            cancellationToken);
        if (TryMaterialize(content, evidenceText, criteria, out result, out parseError))
            return result!;

        throw new InvalidOperationException(parseError);
    }

    private bool TryMaterialize(
        string? content,
        string evidenceText,
        IReadOnlyList<RctCriterion> criteria,
        out AnalysisScoreResult? result,
        out string error)
    {
        result = null;
        if (!TryReadScore(content, out var document, out error))
            return false;

        try
        {
            var parsed = document.Criteria ?? new List<CloudCriterionRow>();
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
            var strengths = RequireStrengths(document.StrengthsPt);
            var question = RequireNarrative(document.InterviewValidationQuestionPt, "interviewValidationQuestionPt", 300);
            var rationale = RequireNarrative(document.Recommendation?.RationalePt, "rationalePt", 500);

            result = new AnalysisScoreResult
            {
                TotalScore = decimal.Round(total, 2, MidpointRounding.AwayFromZero),
                Breakdown = breakdown,
                EngineName = _llm.ModelName,
                PromptVer = PromptVer,
                StampUtc = DateTime.UtcNow,
                UsedLlm = true,
                AssistedDecision = assisted.Decision,
                RationalePt = rationale,
                StrengthsPt = strengths,
                InterviewValidationQuestionPt = question
            };
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("BR-08", StringComparison.Ordinal))
        {
            error = ex.Message;
            return false;
        }
    }

    internal const string SystemPrompt =
        "Es um avaliador de recrutamento. Devolves apenas JSON, sem markdown. " +
        "Prompt " + PromptVer + ". " +
        "Pontua somente os criterios recebidos. note e um numero entre 0 e weight. " +
        "Se note>0, quote e uma citacao contigua exacta do texto (maximo 240 caracteres). " +
        "Se nao ha evidencia, note=0, quote vazio, justificationPt=\"sem evidencia no CV\". " +
        "Justificacao em pt-PT. Nao inventes factos fora do texto. " +
        "O score e input ao RH. decision so pode ser avancar, em_duvida ou nao_avancar. " +
        "Sem evidencia ou conflito no criterio: em_duvida. " +
        "Nao declares seleccao, rejeicao, contratacao ou aprovacao. " +
        "Pesos sao os da lista. Nao inventes pesos nem criterios. " +
        "Nao escrevas nomes, emails ou telefones. " +
        "strengthsPt: 1 a 5 frases curtas. interviewValidationQuestionPt: uma pergunta. rationalePt: justificacao curta da sugestao. " +
        "Formato: {\"criteria\":[{\"code\":\"\",\"note\":0,\"quote\":\"\",\"justificationPt\":\"\",\"conflito\":false}]," +
        "\"recommendation\":{\"decision\":\"em_duvida\",\"rationalePt\":\"\"}," +
        "\"strengthsPt\":[\"\"],\"interviewValidationQuestionPt\":\"\"}";

    internal const string RepairSystemPrompt =
        "Devolves apenas um objeto JSON valido. Sem markdown e sem texto antes ou depois.";

    internal const string RepairUserPrefix =
        "A resposta anterior nao serviu. Return ONLY the JSON object, no markdown. " +
        "Inclui criteria, recommendation.rationalePt, strengthsPt com 1 a 5 frases e interviewValidationQuestionPt. " +
        "Usa somente o texto pseudonimizado abaixo.\n\n";

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

    private static bool TryReadScore(string? content, out CloudScoreDocument document, out string error)
    {
        document = new CloudScoreDocument();
        if (string.IsNullOrWhiteSpace(content))
        {
            error = "BR-08: resposta Qwen vazia.";
            return false;
        }

        if (!LlmJsonObject.TryExtract(content, out var json))
        {
            error = "BR-08: resposta Qwen nao e JSON de score.";
            return false;
        }

        try
        {
            document = JsonSerializer.Deserialize<CloudScoreDocument>(json, JsonOptions)
                ?? new CloudScoreDocument();
            document.StrengthsPt = ReadStrengths(json);
            error = string.Empty;
            return true;
        }
        catch (JsonException)
        {
            error = "BR-08: resposta Qwen nao e JSON de score.";
            return false;
        }
    }

    /// <summary>Accepts a JSON array or a single string. Schema size is checked later.</summary>
    private static List<string>? ReadStrengths(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("strengthsPt", out var node)
            && !doc.RootElement.TryGetProperty("StrengthsPt", out node))
            return null;

        if (node.ValueKind == JsonValueKind.String)
        {
            var text = node.GetString();
            return string.IsNullOrWhiteSpace(text) ? new List<string>() : new List<string> { text };
        }

        if (node.ValueKind != JsonValueKind.Array)
            return new List<string>();

        return node.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .ToList();
    }

    private static IReadOnlyList<string> RequireStrengths(IReadOnlyList<string>? raw)
    {
        var items = (raw ?? Array.Empty<string>())
            .Select(s => s?.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Cast<string>()
            .ToList();
        if (items.Count is < 1 or > 5)
            throw new InvalidOperationException("BR-08: resposta Qwen sem strengthsPt (1 a 5).");

        foreach (var item in items)
        {
            if (item.Length > 180)
                throw new InvalidOperationException("BR-08: strengthsPt demasiado longo.");
            RejectContact(item, "strengthsPt");
            ForbiddenCopyGuard.ThrowIfForbidden(item, "strengthsPt");
        }

        return items;
    }

    private static string RequireNarrative(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"BR-08: resposta Qwen sem {field}.");

        var text = value.Trim();
        if (text.Length > maxLength)
            throw new InvalidOperationException($"BR-08: {field} demasiado longo.");

        RejectContact(text, field);
        ForbiddenCopyGuard.ThrowIfForbidden(text, field);
        return text;
    }

    private static void RejectContact(string text, string field)
    {
        if (text.Contains('@', StringComparison.Ordinal) || PhoneRun.IsMatch(text))
            throw new InvalidOperationException($"BR-08: {field} contem contacto. Nao persistido.");
    }

    private static readonly System.Text.RegularExpressions.Regex PhoneRun = new(
        @"\d{8,}",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Model decision is kept only when it is avancar, em_duvida, or nao_avancar.
    /// The five *_suggest / conflict_review tokens, and selected/rejected/hired/approved/auto_*/pass/fail, are dropped.
    /// </summary>
    public static string? ReadAllowedDecision(string? content)
    {
        if (!LlmJsonObject.TryExtract(content, out var json))
            return null;

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
        var quoteRejected = false;
        if (note > 0)
        {
            if (!RubricEvidenceScorer.TryResolveQuote(evidenceText, quote, out var resolved) || resolved is null)
            {
                // AH-02 still falsifies the criterion. An invented or whitespace-broken
                // quote zeros this row only; the rest of the candidate is scored.
                note = 0;
                quote = null;
                quoteRejected = true;
            }
            else
            {
                quote = resolved.Length > RubricEvidenceScorer.MaxQuoteLength
                    ? resolved[..RubricEvidenceScorer.MaxQuoteLength]
                    : resolved;
            }
        }
        else
        {
            quote = null;
        }

        var justification = quoteRejected
            ? RejectedQuoteJustification
            : string.IsNullOrWhiteSpace(row?.JustificationPt)
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
            JustificationPt = quoteRejected
                ? RejectedQuoteJustification
                : note <= 0 ? HitlCopy.SemEvidencia : justification
        };
    }

    private sealed class CloudScoreDocument
    {
        public List<CloudCriterionRow>? Criteria { get; set; }

        public CloudRecommendation? Recommendation { get; set; }

        [JsonIgnore]
        public List<string>? StrengthsPt { get; set; }

        public string? InterviewValidationQuestionPt { get; set; }
    }

    private sealed class CloudRecommendation
    {
        public string? Decision { get; set; }

        public string? RationalePt { get; set; }
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
