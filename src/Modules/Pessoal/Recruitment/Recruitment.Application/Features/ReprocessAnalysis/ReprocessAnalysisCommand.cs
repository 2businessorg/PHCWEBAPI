using System.Text.Json;
using MediatR;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.EnqueueAnalysis;
using Recruitment.Domain.Repositories;

namespace Recruitment.Application.Features.ReprocessAnalysis;

/// <summary>
/// BR-06: audited reprocess — new stamp; does not erase human selection decisions
/// (those live outside IA columns and are never written by the job).
/// </summary>
public sealed record ReprocessAnalysisCommand(
    string CveStamp,
    string RctStamp,
    string SrtStamp,
    string RequestedBy,
    string? LabGoRef = null) : IRequest<EnqueueAnalysisResultDto>;

public sealed class ReprocessAnalysisCommandHandler
    : IRequestHandler<ReprocessAnalysisCommand, EnqueueAnalysisResultDto>
{
    private readonly IMediator _mediator;
    private readonly ISrtScoreRepository _srt;

    public ReprocessAnalysisCommandHandler(IMediator mediator, ISrtScoreRepository srt)
    {
        _mediator = mediator;
        _srt = srt;
    }

    public async Task<EnqueueAnalysisResultDto> Handle(
        ReprocessAnalysisCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RequestedBy))
            throw new ArgumentException("BR-06: RequestedBy obrigatorio para auditoria.");

        var existing = await _srt.GetByStampAsync(request.SrtStamp, cancellationToken);
        var priorAudit = existing?.JustificationJson;

        var result = await _mediator.Send(
            new EnqueueAnalysisCommand(
                request.CveStamp,
                request.RctStamp,
                request.SrtStamp,
                request.LabGoRef),
            cancellationToken);

        // Append audit marker into message (full audit trail persisted on next successful SaveScore).
        var stamp = DateTime.UtcNow;
        var auditNote = JsonSerializer.Serialize(new
        {
            reprocess_utc = stamp,
            requested_by = request.RequestedBy,
            prior_justification_present = !string.IsNullOrWhiteSpace(priorAudit),
            note = "BR-06: reprocessamento auditado; decisao humana nativa nao e apagada."
        });

        return new EnqueueAnalysisResultDto
        {
            Enqueued = result.Enqueued,
            OutboxId = result.OutboxId,
            CriteriaCount = result.CriteriaCount,
            IntervenienteCount = result.IntervenienteCount,
            Message = result.Enqueued
                ? $"{result.Message} Audit={auditNote}"
                : result.Message
        };
    }
}
