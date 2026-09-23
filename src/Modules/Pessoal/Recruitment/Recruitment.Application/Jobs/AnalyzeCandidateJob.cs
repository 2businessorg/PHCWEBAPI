using System.Text.Encodings.Web;
using System.Text.Json;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recruitment.Application.DTOs;
using Recruitment.Application.Options;
using Recruitment.Application.Privacy;
using Recruitment.Application.Scoring;
using Recruitment.Application.Services;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;
using Recruitment.Domain.Repositories;
using Shared.Abstractions.DocumentTextExtraction;

namespace Recruitment.Application.Jobs;

/// <summary>
/// Hangfire job name: Recruitment.AnalyzeCandidate (BR-12 queue owner = PHCAPI.Host).
/// Enforces AH-01â€¦AH-08 and BR-02â€¦BR-05 / BR-07 / BR-11 on the Analisar path.
/// </summary>
public sealed class AnalyzeCandidateJob
{
    public const string JobName = "Recruitment.AnalyzeCandidate";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IRecruitmentOutboxRepository _outbox;
    private readonly ICvAnexoRepository _anexos;
    private readonly IRctCriteriaRepository _criteria;
    private readonly IRctIntervenienteRepository _intervenientes;
    private readonly ISrtScoreRepository _srtScores;
    private readonly ICveEstadoIaRepository _cveEstado;
    private readonly IDocumentTextExtractor _textExtractor;
    private readonly ICandidateScoreEngineSelector _engines;
    private readonly IPhcAvisoService _avisos;
    private readonly IOptions<RecruitmentIaOptions> _options;
    private readonly IRecruitmentCloudEgressGuard _cloudEgress;
    private readonly ILogger<AnalyzeCandidateJob> _logger;

    public AnalyzeCandidateJob(
        IRecruitmentOutboxRepository outbox,
        ICvAnexoRepository anexos,
        IRctCriteriaRepository criteria,
        IRctIntervenienteRepository intervenientes,
        ISrtScoreRepository srtScores,
        ICveEstadoIaRepository cveEstado,
        IDocumentTextExtractor textExtractor,
        ICandidateScoreEngineSelector engines,
        IPhcAvisoService avisos,
        IOptions<RecruitmentIaOptions> options,
        IRecruitmentCloudEgressGuard cloudEgress,
        ILogger<AnalyzeCandidateJob> logger)
    {
        _outbox = outbox;
        _anexos = anexos;
        _criteria = criteria;
        _intervenientes = intervenientes;
        _srtScores = srtScores;
        _cveEstado = cveEstado;
        _textExtractor = textExtractor;
        _engines = engines;
        _avisos = avisos;
        _options = options;
        _cloudEgress = cloudEgress;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public Task ExecuteAsync(long outboxId) => ExecuteAsync(outboxId, CancellationToken.None);

    public async Task ExecuteAsync(long outboxId, CancellationToken ct)
    {
        var item = await _outbox.GetAsync(outboxId, ct)
            ?? throw new InvalidOperationException($"Outbox {outboxId} nao encontrada.");

        try
        {
            await ExecuteCoreAsync(item, outboxId, ct);
        }
        catch (OperationCanceledException)
        {
            // E2 / AH-08: cancel/abort must ClearScore fully (not PARTIAL leftover).
            await ClearScoreReliableAsync(item.SrtStamp, CancellationToken.None);
            try
            {
                await _cveEstado.SetEstadoIaAsync(item.CveStamp, IaEstados.Erro, CancellationToken.None);
                await _outbox.MarkErrorAsync(outboxId, "cancelado/abort", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cleanup apos cancel/abort falhou outbox={Id}", outboxId);
            }

            throw;
        }
    }

    private async Task ClearScoreReliableAsync(string srtStamp, CancellationToken ct)
    {
        await _srtScores.ClearScoreOnOcrErrorAsync(srtStamp, ct);
    }

    private async Task ExecuteCoreAsync(Domain.Entities.RecruitmentOutboxItem item, long outboxId, CancellationToken ct)
    {
        await _outbox.MarkProcessingAsync(outboxId, ct);
        await _outbox.HeartbeatAsync(outboxId, ct);
        await _cveEstado.SetEstadoIaAsync(item.CveStamp, IaEstados.Pendente, ct);

        // Selection snapshot before job â€” BR-03 evidence for tests / ops diffs.
        var srtBefore = await _srtScores.GetByStampAsync(item.SrtStamp, ct);
        var selectionBefore = srtBefore?.SelectionStateSnapshot;
        var condpBefore = srtBefore?.Condp;

        var recipients = await _intervenientes.GetIntervenientesAsync(item.RctStamp, ct);
        if (recipients.Count == 0 && string.IsNullOrWhiteSpace(item.LabGoRef))
        {
            // Should not happen if BR-01 held; fail closed.
            await FailAsync(item, "BR-01/BR-05: Analisar sem RCTCLB e sem GO lab.", ct);
            return;
        }

        var anexo = await _anexos.GetCvAnexoAsync(item.CveStamp, ct);
        if (anexo is null || anexo.Bytes.Length == 0)
        {
            await FailAsync(item, "K6: anexo CV em falta.", ct);
            await NotifyAsync(item, IaEstados.Erro, recipients, ct);
            return;
        }

        DocumentTextExtractionResult ocr;
        await using (var stream = new MemoryStream(anexo.Bytes, writable: false))
        {
            ocr = await _textExtractor.ExtractAsync(
                stream,
                anexo.FileName,
                anexo.ContentType,
                ct);
        }

        // Never log CV body (BR-08).
        _logger.LogInformation(
            "OCR outbox={OutboxId} success={Success} source={Source} engine={Engine} textLen={Len}",
            outboxId, ocr.Success, ocr.Source, ocr.EngineName, ocr.Text?.Length ?? 0);

        if (!ocr.Success || string.IsNullOrWhiteSpace(ocr.Text))
        {
            // AH-08 / BR-07: estado_ia=erro, NO score invented.
            await ClearScoreReliableAsync(item.SrtStamp, ct);
            await _cveEstado.SetEstadoIaAsync(item.CveStamp, IaEstados.Erro, ct);
            await _outbox.MarkErrorAsync(outboxId, ocr.Error ?? "OCR falhou ou u_texto vazio.", ct);
            await NotifyAsync(item, IaEstados.Erro, recipients, ct);
            await AssertNoSideEffectsAsync(item.SrtStamp, selectionBefore, condpBefore, ct);
            return;
        }

        await _anexos.SaveTextoAsync(anexo.AnexoStamp, ocr.Text, ct);

        var egress = await _cloudEgress.PrepareAsync(
            sessionId: item.SrtStamp,
            documentId: anexo.AnexoStamp,
            plainText: ocr.Text!,
            cancellationToken: ct);

        if (_engines.CloudEnabled && !egress.EgressAllowed)
        {
            var reason = string.IsNullOrWhiteSpace(egress.FailureReason)
                ? "BR-08/BR-11: egress Presidio recusado."
                : $"BR-08/BR-11: {egress.FailureReason}";
            _logger.LogWarning(
                "Cloud egress refused. outbox={Id} reason={Reason}",
                outboxId,
                egress.FailureReason);
            await ClearScoreReliableAsync(item.SrtStamp, ct);
            await FailAsync(item, reason, ct);
            await NotifyAsync(item, IaEstados.Erro, recipients, ct);
            await AssertNoSideEffectsAsync(item.SrtStamp, selectionBefore, condpBefore, ct);
            return;
        }

        var criteria = await _criteria.GetUsableCriteriaAsync(item.RctStamp, ct);
        if (criteria.Count < 1)
        {
            await FailAsync(item, "AH-01: lista crt vazia no job.", ct);
            await NotifyAsync(item, IaEstados.Erro, recipients, ct);
            return;
        }

        var evidenceText = _engines.CloudEnabled
            ? egress.PseudonymizedText ?? string.Empty
            : ocr.Text!;
        if (_engines.CloudEnabled && string.IsNullOrWhiteSpace(evidenceText))
        {
            await ClearScoreReliableAsync(item.SrtStamp, ct);
            await FailAsync(item, "BR-08: texto pseudonimizado vazio.", ct);
            await NotifyAsync(item, IaEstados.Erro, recipients, ct);
            await AssertNoSideEffectsAsync(item.SrtStamp, selectionBefore, condpBefore, ct);
            return;
        }

        AnalysisScoreResult score;
        try
        {
            score = await _engines.Resolve().ScoreAsync(evidenceText, criteria, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Score falhou outbox={OutboxId} cloud={Cloud}", outboxId, _engines.CloudEnabled);
            var message = ScoreFailureMessage(ex);
            if (_engines.CloudEnabled)
                await ClearScoreReliableAsync(item.SrtStamp, ct);
            await FailAsync(item, message, ct);
            await NotifyAsync(item, IaEstados.Erro, recipients, ct);
            return;
        }

        // AH-02: note>0 quote must be a subset of the text that was scored
        // (raw OCR offline, pseudonymized text when Qwen ran).
        foreach (var row in score.Breakdown.Where(b => b.Note > 0))
        {
            RubricEvidenceScorer.AssertQuoteIsSubset(evidenceText, row.Quote);
            ForbiddenCopyGuard.ThrowIfForbidden(row.JustificationPt, row.Code);
        }

        var assisted = AssistedRecommendationBuilder.Build(score.Breakdown);
        var payload = new JustificationPayloadDto
        {
            Engine = score.EngineName,
            PromptVer = score.PromptVer,
            StampUtc = score.StampUtc,
            UsedLlm = score.UsedLlm,
            Total = score.TotalScore,
            ScoreIsInputNotDecision = true,
            HumanDecisionRequired = true,
            DisclaimerPt = AssistedDecisions.DisclaimerPt,
            CandidateAlias = CandidateAlias(item.SrtStamp),
            Recommendation = new AssistedRecommendationDto
            {
                Decision = assisted.Decision,
                LabelPt = assisted.LabelPt,
                DecisionNote = assisted.DecisionNote
            },
            Criteria = assisted.Criteria,
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

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        ForbiddenCopyGuard.ThrowIfForbidden(json, "justificationJson");

        if (score.UsedLlm)
        {
            _logger.LogInformation(
                "Qwen score outbox={Id} model={Model} egressAllowed={Allowed} entityCount={Count} leakCheckPassed={Leak}",
                outboxId,
                score.EngineName,
                egress.EgressAllowed,
                egress.Result?.Entities.Count ?? 0,
                egress.Result?.LeakCheck.Passed);
        }

        var audit = JsonSerializer.Serialize(BuildAudit(outboxId, score, egress, item.SrtStamp, anexo.AnexoStamp), JsonOptions);

        // BR-02: persist on SRT only. BR-03/04/09: repository must not write selection/condp.
        await _srtScores.SaveScoreAsync(
            item.SrtStamp,
            score.TotalScore,
            json,
            score.EngineName,
            score.PromptVer,
            score.StampUtc,
            audit,
            ct);

        await _cveEstado.SetEstadoIaAsync(item.CveStamp, IaEstados.Ok, ct);
        await _outbox.MarkDoneAsync(outboxId, ct);
        await NotifyAsync(item, IaEstados.Ok, recipients, ct);
        await AssertNoSideEffectsAsync(item.SrtStamp, selectionBefore, condpBefore, ct);
    
    }


    private static string CandidateAlias(string srtStamp)
    {
        var stamp = (srtStamp ?? string.Empty).Trim();
        return stamp.Length == 0 ? "cand-unknown" : "cand-" + stamp;
    }

    private string ScoreFailureMessage(Exception ex)
    {
        if (!_engines.CloudEnabled)
            return ex.Message;

        if (ex.Message.StartsWith("AH-02", StringComparison.Ordinal)
            || ex.Message.StartsWith("BR-08", StringComparison.Ordinal))
            return ex.Message;

        return "BR-08: chamada Qwen falhou.";
    }

    private object BuildAudit(
        long outboxId,
        AnalysisScoreResult score,
        CloudEgressPreparation egress,
        string sessionId,
        string documentId)
    {
        var entities = egress.Result?.Entities;
        var entityTypes = new Dictionary<string, int>(StringComparer.Ordinal);
        if (entities is not null)
        {
            foreach (var entity in entities)
            {
                var type = string.IsNullOrWhiteSpace(entity.EntityType) ? "UNKNOWN" : entity.EntityType;
                entityTypes[type] = entityTypes.TryGetValue(type, out var n) ? n + 1 : 1;
            }
        }

        return new
        {
            outbox_id = outboxId,
            analyzed_utc = score.StampUtc,
            host = _options.Value.QueueHostName,
            ah = "AH-01..AH-08 enforced",
            used_llm = score.UsedLlm,
            engine = score.EngineName,
            prompt_ver = score.PromptVer,
            model = score.EngineName,
            job_stamp_utc = score.StampUtc,
            ocr_stamp_utc = score.StampUtc,
            egress_allowed = egress.EgressAllowed,
            entity_count = entities?.Count ?? 0,
            entity_types = entityTypes,
            leak_check_passed = egress.Result?.LeakCheck.Passed,
            failure_reason = egress.FailureReason,
            session_id = egress.Result?.SessionId ?? sessionId,
            document_id = egress.Result?.DocumentId ?? documentId
        };
    }

    private async Task FailAsync(Domain.Entities.RecruitmentOutboxItem item, string message, CancellationToken ct)
    {
        await _cveEstado.SetEstadoIaAsync(item.CveStamp, IaEstados.Erro, ct);
        await _outbox.MarkErrorAsync(item.Id, message, ct);
        _logger.LogWarning("Analisar erro outbox={Id}: {Message}", item.Id, message);
    }

    private async Task NotifyAsync(
        Domain.Entities.RecruitmentOutboxItem item,
        string estadoIa,
        IReadOnlyList<Domain.Entities.RctInterveniente> recipients,
        CancellationToken ct)
    {
        // BR-05: aviso obrigatorio quando Analisar corre.
        await _avisos.EmitAnalisarCompletedAsync(
            item.RctStamp,
            item.CveStamp,
            estadoIa,
            recipients,
            ct);
    }

    private async Task AssertNoSideEffectsAsync(
        string srtStamp,
        string? selectionBefore,
        string? condpBefore,
        CancellationToken ct)
    {
        var after = await _srtScores.GetByStampAsync(srtStamp, ct);
        if (after is null)
            return;

        if (!string.Equals(selectionBefore, after.SelectionStateSnapshot, StringComparison.Ordinal))
            throw new InvalidOperationException("BR-03/K3: job alterou estado de seleccao.");

        if (!string.Equals(condpBefore, after.Condp, StringComparison.Ordinal))
            throw new InvalidOperationException("BR-04: job alterou srt.condp.");
    }
}
