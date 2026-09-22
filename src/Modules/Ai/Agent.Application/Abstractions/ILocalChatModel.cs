using Agent.Application.Chat;

namespace Agent.Application.Abstractions;

/// <summary>
/// Local chat model abstraction. Implementations may use Ollama, vLLM, llama.cpp, etc.
/// </summary>
public interface ILocalChatModel
{
    string ModelName { get; }

    Task<ChatModelResponse> SendAsync(
        IReadOnlyCollection<ChatMessage> messages,
        IReadOnlyCollection<ToolDefinition> tools,
        CancellationToken cancellationToken,
        ChatCompletionOptions? completion = null);
}
