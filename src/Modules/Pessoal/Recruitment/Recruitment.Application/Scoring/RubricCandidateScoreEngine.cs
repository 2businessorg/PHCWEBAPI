using Recruitment.Domain.Entities;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Offline engine. Used only when RecruitmentIa:EnableCloudLlm is false.
/// </summary>
public sealed class RubricCandidateScoreEngine : ICandidateScoreEngine
{
    private readonly IRubricEvidenceScorer _scorer;

    public RubricCandidateScoreEngine(IRubricEvidenceScorer scorer)
    {
        _scorer = scorer;
    }

    public Task<AnalysisScoreResult> ScoreAsync(
        string evidenceText,
        IReadOnlyList<RctCriterion> criteria,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_scorer.Score(evidenceText, criteria));
    }
}
