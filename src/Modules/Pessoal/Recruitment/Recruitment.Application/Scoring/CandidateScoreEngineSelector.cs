using Microsoft.Extensions.Options;
using Recruitment.Application.Options;

namespace Recruitment.Application.Scoring;

public sealed class CandidateScoreEngineSelector : ICandidateScoreEngineSelector
{
    private readonly RecruitmentIaOptions _options;
    private readonly RubricCandidateScoreEngine _rubric;
    private readonly QwenCloudScoreEngine _cloud;

    public CandidateScoreEngineSelector(
        IOptions<RecruitmentIaOptions> options,
        RubricCandidateScoreEngine rubric,
        QwenCloudScoreEngine cloud)
    {
        _options = options.Value;
        _rubric = rubric;
        _cloud = cloud;
    }

    public bool CloudEnabled => _options.EnableCloudLlm;

    public ICandidateScoreEngine Resolve() => CloudEnabled ? _cloud : _rubric;
}
