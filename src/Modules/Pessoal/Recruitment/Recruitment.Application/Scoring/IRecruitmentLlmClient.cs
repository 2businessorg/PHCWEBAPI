namespace Recruitment.Application.Scoring;

/// <summary>
/// Recruitment port over the shared chat model (Qwen via ILocalChatModel).
/// Implementations must not log prompt or CV text.
/// </summary>
public interface IRecruitmentLlmClient
{
    string ModelName { get; }

    Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken);
}
