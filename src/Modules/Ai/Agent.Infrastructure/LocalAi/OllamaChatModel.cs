using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Agent.Application.Abstractions;
using Agent.Application.Chat;
using Agent.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Agent.Infrastructure.LocalAi;

/// <summary>
/// Ollama implementation of <see cref="ILocalChatModel"/> using the local /api/chat endpoint.
/// </summary>
public sealed class OllamaChatModel : ILocalChatModel
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly LocalAiOptions _options;
    private readonly ILogger<OllamaChatModel> _logger;

    public OllamaChatModel(
        HttpClient httpClient,
        IOptions<LocalAiOptions> options,
        ILogger<OllamaChatModel> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public string ModelName => _options.Model;

    public async Task<ChatModelResponse> SendAsync(
        IReadOnlyCollection<ChatMessage> messages,
        IReadOnlyCollection<ToolDefinition> tools,
        CancellationToken cancellationToken,
        ChatCompletionOptions? completion = null)
    {
        var payload = new OllamaChatRequest
        {
            Model = _options.Model,
            Stream = false,
            Think = _options.Think,
            Format = completion?.JsonObjectResponse == true ? "json" : null,
            Messages = messages.Select(ToOllamaMessage).ToList(),
            Tools = tools.Select(ToOllamaTool).ToList()
        };

        using var response = await _httpClient.PostAsJsonAsync("api/chat", payload, JsonOptions, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Ollama chat failed. Status={Status} BodyLength={Length}", (int)response.StatusCode, body.Length);
            throw new InvalidOperationException($"Ollama returned {(int)response.StatusCode}.");
        }

        var parsed = JsonSerializer.Deserialize<OllamaChatResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException("Ollama returned an empty chat response.");

        var toolCalls = ParseToolCalls(parsed.Message?.ToolCalls);
        return new ChatModelResponse(parsed.Message?.Content, toolCalls);
    }

    private static OllamaMessage ToOllamaMessage(ChatMessage message)
    {
        var ollama = new OllamaMessage
        {
            Role = message.Role,
            Content = message.Content
        };

        if (string.Equals(message.Role, "tool", StringComparison.OrdinalIgnoreCase))
        {
            ollama.ToolName = message.ToolCallId;
        }

        if (message.ToolCalls is { Count: > 0 })
        {
            ollama.ToolCalls = message.ToolCalls.Select(c => new OllamaToolCall
            {
                Function = new OllamaFunctionCall
                {
                    Name = c.Name,
                    Arguments = c.Arguments
                }
            }).ToList();
        }

        return ollama;
    }

    private static OllamaTool ToOllamaTool(ToolDefinition tool)
        => new()
        {
            Type = "function",
            Function = new OllamaToolFunction
            {
                Name = tool.Name,
                Description = tool.Description,
                Parameters = tool.InputSchema
            }
        };

    private static IReadOnlyList<ToolCall> ParseToolCalls(IReadOnlyList<OllamaToolCall>? calls)
    {
        if (calls is null || calls.Count == 0)
            return Array.Empty<ToolCall>();

        var result = new List<ToolCall>(calls.Count);
        var index = 0;
        foreach (var call in calls)
        {
            var name = call.Function?.Name;
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var arguments = NormalizeArguments(call.Function?.Arguments);
            result.Add(new ToolCall($"call_{index++}", name, arguments));
        }

        return result;
    }

    private static JsonElement NormalizeArguments(JsonElement? arguments)
    {
        if (arguments is null || arguments.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return JsonDocument.Parse("{}").RootElement.Clone();

        if (arguments.Value.ValueKind == JsonValueKind.String)
        {
            var raw = arguments.Value.GetString();
            if (string.IsNullOrWhiteSpace(raw))
                return JsonDocument.Parse("{}").RootElement.Clone();

            using var parsed = JsonDocument.Parse(raw);
            return parsed.RootElement.Clone();
        }

        return arguments.Value.Clone();
    }

    private sealed class OllamaChatRequest
    {
        public string Model { get; set; } = string.Empty;
        public bool Stream { get; set; }
        public bool Think { get; set; }
        public string? Format { get; set; }
        public List<OllamaMessage> Messages { get; set; } = [];
        public List<OllamaTool> Tools { get; set; } = [];
    }

    private sealed class OllamaMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        [JsonPropertyName("tool_name")]
        public string? ToolName { get; set; }
        [JsonPropertyName("tool_calls")]
        public List<OllamaToolCall>? ToolCalls { get; set; }
    }

    private sealed class OllamaTool
    {
        public string Type { get; set; } = "function";
        public OllamaToolFunction Function { get; set; } = new();
    }

    private sealed class OllamaToolFunction
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public JsonElement Parameters { get; set; }
    }

    private sealed class OllamaChatResponse
    {
        public OllamaMessage? Message { get; set; }
    }

    private sealed class OllamaToolCall
    {
        public OllamaFunctionCall? Function { get; set; }
    }

    private sealed class OllamaFunctionCall
    {
        public string? Name { get; set; }
        public JsonElement Arguments { get; set; }
    }
}
