using Agent.Application.Abstractions;
using Agent.Application.Chat;
using Microsoft.Extensions.Options;
using Recruitment.Application.Scoring;
using Recruitment.Infrastructure.Options;

namespace Recruitment.Infrastructure.Scoring;

/// <summary>
/// Adapter: Recruitment cloud score -> existing ILocalChatModel (Qwen compatible-mode).
/// Does not open its own HTTP client.
/// </summary>
public sealed class LocalChatRecruitmentLlmClient : IRecruitmentLlmClient
{
    private readonly ILocalChatModel _chat;
    private readonly RecruitmentLocalAiOptions _options;

    public LocalChatRecruitmentLlmClient(
        ILocalChatModel chat,
        IOptions<RecruitmentLocalAiOptions> options)
    {
        _chat = chat;
        _options = options.Value;
    }

    public string ModelName => string.IsNullOrWhiteSpace(_chat.ModelName)
        ? _options.Model
        : _chat.ModelName;

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("BR-08: LocalAI ApiKey em falta. Qwen nao foi chamado.");

        if (string.IsNullOrWhiteSpace(ModelName))
            throw new InvalidOperationException("BR-08: LocalAI Model em falta.");

        var messages = new ChatMessage[]
        {
            new("system", systemPrompt),
            new("user", userPrompt)
        };

        var response = await _chat.SendAsync(
            messages,
            Array.Empty<ToolDefinition>(),
            cancellationToken,
            ChatCompletionOptions.JsonObject);

        if (string.IsNullOrWhiteSpace(response.Content))
            throw new InvalidOperationException("BR-08: Qwen devolveu conteudo vazio.");

        return response.Content;
    }
}
