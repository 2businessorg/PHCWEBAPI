namespace Recruitment.Application.Options;

/// <summary>
/// Recruitment x IA options. Cloud LLM defaults OFF (BR-08).
/// </summary>
public sealed class RecruitmentIaOptions
{
    public const string SectionName = "RecruitmentIa";

    /// <summary>Master switch for enqueue / analyze path.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Cloud LLM path — must stay false unless Denilson GO (BR-08).</summary>
    public bool EnableCloudLlm { get; set; }

    /// <summary>Hangfire job display name / recurring id documentation.</summary>
    public string HangfireJobName { get; set; } = "Recruitment.AnalyzeCandidate";

    /// <summary>Orphan TTL for estado_ia=pendente / outbox processing (BR-12).</summary>
    public int OrphanTtlMinutes { get; set; } = 60;

    /// <summary>Allow enqueue without RCTCLB only when LabGoRef is set (BR-01 exception).</summary>
    public bool AllowLabEnqueueWithoutRctclb { get; set; }

    public string? LabGoRef { get; set; }

    /// <summary>Named queue host for BR-12 (this Hangfire host).</summary>
    public string QueueHostName { get; set; } = "PHCAPI.Host Hangfire (PHCHANGFIRE schema)";
}
