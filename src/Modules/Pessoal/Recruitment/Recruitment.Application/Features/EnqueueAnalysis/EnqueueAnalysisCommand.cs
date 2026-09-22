using MediatR;
using Recruitment.Application.DTOs;
using Recruitment.Application.Services;

namespace Recruitment.Application.Features.EnqueueAnalysis;

public sealed record EnqueueAnalysisCommand(
    string CveStamp,
    string RctStamp,
    string SrtStamp,
    string? LabGoRef = null) : IRequest<EnqueueAnalysisResultDto>;

public sealed class EnqueueAnalysisCommandHandler
    : IRequestHandler<EnqueueAnalysisCommand, EnqueueAnalysisResultDto>
{
    private readonly IRecruitmentEnqueueService _enqueue;
    private readonly IRecruitmentJobScheduler _scheduler;

    public EnqueueAnalysisCommandHandler(
        IRecruitmentEnqueueService enqueue,
        IRecruitmentJobScheduler scheduler)
    {
        _enqueue = enqueue;
        _scheduler = scheduler;
    }

    public async Task<EnqueueAnalysisResultDto> Handle(
        EnqueueAnalysisCommand request,
        CancellationToken cancellationToken)
    {
        var decision = await _enqueue.TryEnqueueAsync(
            request.CveStamp,
            request.RctStamp,
            request.SrtStamp,
            request.LabGoRef,
            cancellationToken);

        if (decision.Enqueued && decision.OutboxId is long id)
            _scheduler.EnqueueAnalyze(id);

        return new EnqueueAnalysisResultDto
        {
            Enqueued = decision.Enqueued,
            OutboxId = decision.OutboxId,
            Message = decision.Message,
            CriteriaCount = decision.CriteriaCount,
            IntervenienteCount = decision.IntervenienteCount
        };
    }
}

/// <summary>Abstracts Hangfire so Application stays testable.</summary>
public interface IRecruitmentJobScheduler
{
    void EnqueueAnalyze(long outboxId);
}
