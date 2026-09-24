using Recruitment.Domain.Entities;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Strategy for scoring one candidature against RCT criteria.
/// Rubric (offline) and Qwen cloud are separate implementations.
/// </summary>
public interface ICandidateScoreEngine
{
    Task<AnalysisScoreResult> ScoreAsync(
        string evidenceText,
        IReadOnlyList<RctCriterion> criteria,
        CancellationToken cancellationToken);
}

/// <summary>
/// Selects the score engine from RecruitmentIa:EnableCloudLlm.
/// </summary>
public interface ICandidateScoreEngineSelector
{
    bool CloudEnabled { get; }

    ICandidateScoreEngine Resolve();
}
