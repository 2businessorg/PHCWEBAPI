namespace Recruitment.Infrastructure.Options;

/// <summary>
/// Reads the shared LocalAI section (Qwen). ApiKey is checked, never logged.
/// </summary>
public sealed class RecruitmentLocalAiOptions
{
    public const string SectionName = "LocalAI";

    public string? ApiKey { get; set; }

    public string Model { get; set; } = string.Empty;
}
