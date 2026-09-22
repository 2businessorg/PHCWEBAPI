using Hangfire;
using Recruitment.Application.Features.EnqueueAnalysis;
using Recruitment.Application.Jobs;

namespace Recruitment.Infrastructure;

/// <summary>
/// Hangfire scheduler — queue owner is PHCAPI.Host (BR-12).
/// </summary>
public sealed class HangfireRecruitmentJobScheduler : IRecruitmentJobScheduler
{
    private readonly IBackgroundJobClient _jobs;

    public HangfireRecruitmentJobScheduler(IBackgroundJobClient jobs)
    {
        _jobs = jobs;
    }

    public void EnqueueAnalyze(long outboxId)
    {
        _jobs.Enqueue<AnalyzeCandidateJob>(job => job.ExecuteAsync(outboxId));
    }
}
