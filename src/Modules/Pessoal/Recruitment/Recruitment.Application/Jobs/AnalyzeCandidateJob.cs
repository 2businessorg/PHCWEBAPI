using System.Text.Json;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recruitment.Application.DTOs;
using Recruitment.Application.Options;
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
        WriteIndented = false
    };

    private readonly IRecruitmentOutboxRepository _outbox;
    private readonly ICvAnexoRepository _anexos;
    private readonly IRctCriteriaRepository _criteria;
    private readonly IRctIntervenienteRepository _intervenientes;
    private readonly ISrtScoreRepository _srtScores;
    private readonly ICveEstadoIaRepository _cveEstado;
    private readonly IDocumentTextExtractor _textExtractor;
    private readonly IRubricEvidenceScorer _scorer;
    private readonly IPhcAvisoService _avisos;
    private readonly IOptions<RecruitmentIaOptions> _options;
    private readonly ILogger<AnalyzeCandidateJob> _logger;

    public AnalyzeCandidateJob(
        IRecruitmentOutboxRepository outbox,
        ICvAnexoRepository anexos,
        IRctCriteriaRepository criteria,
        IRctIntervenienteRepository intervenientes,
        ISrtScoreRepository srtScores,
        ICveEstadoIaRepository cveEstado,
        IDocumentTextExtractor textExtractor,
        IRubricEvidenceScorer scorer,
        IPhcAvisoService avisos,
        IOptions<RecruitmentIaOptions> options,
        ILogger<AnalyzeCandidateJob> logger)
    {
        _outbox = outbox;
        _anexos = anexos;
        _criteria = criteria;
        _intervenientes = intervenientes;
        _srtScores = srtScores;
        _cveEstado = cveEstado;
        _textExtractor = textExtractor;
        _scorer = scorer;
        _avisos = avisos;
        _options = options;
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

        if (_options.Value.EnableCloudLlm)
        {
            // Soft gate: v1 path remains deterministic unless GO + future adapter.
            _logger.LogWarning(
                "EnableCloudLlm=true but v1 uses deterministic rubric only (BR-08/BR-11). outbox={Id}",
                outboxId);
        }

        var criteria = await _criteria.GetUsableCriteriaAsync(item.RctStamp, ct);
        if (criteria.Count < 1)
        {
            await FailAsync(item, "AH-01: lista crt vazia no job.", ct);
            await NotifyAsync(item, IaEstados.Erro, recipients, ct);
            return;
        }

        AnalysisScoreResult score;
        try
        {
            score = _scorer.Score(ocr.Text, criteria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Score falhou outbox={OutboxId}", outboxId);
            await FailAsync(item, ex.Message, ct);
            await NotifyAsync(item, IaEstados.Erro, recipients, ct);
            return;
        }

        // AH-02 kill: every note>0 must have quote âŠ† u_texto
        foreach (var row in score.Breakdown.Where(b => b.Note > 0))
        {
            RubricEvidenceScorer.AssertQuoteIsSubset(ocr.Text, row.Quote);
            ForbiddenCopyGuard.ThrowIfForbidden(row.JustificationPt, row.Code);
        }

        var payload = new JustificationPayloadDto
        {
            Engine = score.EngineName,
            PromptVer = score.PromptVer,
            StampUtc = score.StampUtc,
            UsedLlm = score.UsedLlm,
            Total = score.TotalScore,
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

        var audit = JsonSerializer.Serialize(new
        {
            outbox_id = outboxId,
            analyzed_utc = score.StampUtc,
            host = _options.Value.QueueHostName,
            ah = "AH-01..AH-08 enforced"
        }, JsonOptions);

        // BR-02: persist on SRT only. BR-03/04: repository must not write selection/condp.
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
