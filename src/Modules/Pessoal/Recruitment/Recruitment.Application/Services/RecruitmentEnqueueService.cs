using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recruitment.Application.Options;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;
using Recruitment.Domain.Repositories;

namespace Recruitment.Application.Services;

/// <summary>
/// BR-01: enqueue only when crt≥1 AND (RCTCLB≥1 OR lab GO exception).
/// </summary>
public sealed class RecruitmentEnqueueService : IRecruitmentEnqueueService
{
    private readonly IRctCriteriaRepository _criteria;
    private readonly IRctIntervenienteRepository _intervenientes;
    private readonly ICvAnexoRepository _anexos;
    private readonly IRecruitmentOutboxRepository _outbox;
    private readonly ICveEstadoIaRepository _cveEstado;
    private readonly IOptions<RecruitmentIaOptions> _options;
    private readonly ILogger<RecruitmentEnqueueService> _logger;

    public RecruitmentEnqueueService(
        IRctCriteriaRepository criteria,
        IRctIntervenienteRepository intervenientes,
        ICvAnexoRepository anexos,
        IRecruitmentOutboxRepository outbox,
        ICveEstadoIaRepository cveEstado,
        IOptions<RecruitmentIaOptions> options,
        ILogger<RecruitmentEnqueueService> logger)
    {
        _criteria = criteria;
        _intervenientes = intervenientes;
        _anexos = anexos;
        _outbox = outbox;
        _cveEstado = cveEstado;
        _options = options;
        _logger = logger;
    }

    public async Task<EnqueueDecision> TryEnqueueAsync(
        string cveStamp,
        string rctStamp,
        string srtStamp,
        string? labGoRef = null,
        CancellationToken ct = default)
    {
        var opts = _options.Value;
        if (!opts.Enabled)
        {
            return new EnqueueDecision
            {
                Enqueued = false,
                Message = "RecruitmentIa desativado na configuracao."
            };
        }

        var crtCount = await _criteria.CountUsableCriteriaAsync(rctStamp, ct);
        var clbCount = await _intervenientes.CountIntervenientesAsync(rctStamp, ct);

        var effectiveGo = labGoRef ?? opts.LabGoRef;
        var labException = opts.AllowLabEnqueueWithoutRctclb
            && !string.IsNullOrWhiteSpace(effectiveGo);

        if (crtCount < 1)
        {
            _logger.LogInformation(
                "BR-01: enqueue recusado — crt={Crt} rct={Rct}", crtCount, rctStamp);
            return new EnqueueDecision
            {
                Enqueued = false,
                CriteriaCount = crtCount,
                IntervenienteCount = clbCount,
                Message = "Sem caracteristicas (crt) utilizaveis no RCT. IA nao entra em Analisar."
            };
        }

        if (clbCount < 1 && !labException)
        {
            _logger.LogInformation(
                "BR-01: enqueue recusado — rctclb={Clb} rct={Rct}", clbCount, rctStamp);
            return new EnqueueDecision
            {
                Enqueued = false,
                CriteriaCount = crtCount,
                IntervenienteCount = clbCount,
                Message = "Sem interveniente RCTCLB. IA nao entra em Analisar (excepto lab+GO Denilson)."
            };
        }

        var anexo = await _anexos.GetCvAnexoAsync(cveStamp, ct);
        if (anexo is null || anexo.Bytes.Length == 0)
        {
            return new EnqueueDecision
            {
                Enqueued = false,
                CriteriaCount = crtCount,
                IntervenienteCount = clbCount,
                Message = "K6: sem anexo CV (oritable=cve). Analise proibida."
            };
        }

        var item = new RecruitmentOutboxItem
        {
            CveStamp = cveStamp,
            RctStamp = rctStamp,
            SrtStamp = srtStamp,
            AnexoStamp = anexo.AnexoStamp,
            Estado = OutboxEstados.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            LabGoRef = labException ? effectiveGo : null
        };

        var id = await _outbox.TryEnqueueAsync(item, ct);
        if (id is null)
        {
            return new EnqueueDecision
            {
                Enqueued = false,
                CriteriaCount = crtCount,
                IntervenienteCount = clbCount,
                Message = "Outbox nao criada (duplicado ou falha de persistencia)."
            };
        }

        await _cveEstado.SetEstadoIaAsync(cveStamp, IaEstados.Pendente, ct);

        _logger.LogInformation(
            "BR-01/BR-12: outbox {OutboxId} enfileirada cve={Cve} rct={Rct} host={Host}",
            id, cveStamp, rctStamp, opts.QueueHostName);

        return new EnqueueDecision
        {
            Enqueued = true,
            OutboxId = id,
            CriteriaCount = crtCount,
            IntervenienteCount = clbCount,
            Message = "Outbox criada. Job Hangfire Recruitment.AnalyzeCandidate."
        };
    }
}
