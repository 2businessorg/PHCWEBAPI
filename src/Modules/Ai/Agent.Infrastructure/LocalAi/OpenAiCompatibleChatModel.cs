using System.Net.Http.Headers;
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
/// OpenAI-compatible chat client (Qwen DashScope compatible-mode).
/// </summary>
public sealed class OpenAiCompatibleChatModel : ILocalChatModel
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly LocalAiOptions _options;
    private readonly ILogger<OpenAiCompatibleChatModel> _logger;

    public OpenAiCompatibleChatModel(
        HttpClient httpClient,
        IOptions<LocalAiOptions> options,
        ILogger<OpenAiCompatibleChatModel> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        var apiKey = (_options.ApiKey ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    public string ModelName => _options.Model;

    public async Task<ChatModelResponse> SendAsync(
        IReadOnlyCollection<ChatMessage> messages,
        IReadOnlyCollection<ToolDefinition> tools,
        CancellationToken cancellationToken,
        ChatCompletionOptions? completion = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["model"] = _options.Model,
            ["stream"] = false,
            ["messages"] = messages.Select(ToOpenAiMessage).ToList()
        };

        var isCoder = _options.Model.Contains("coder", StringComparison.OrdinalIgnoreCase);
        if (!isCoder)
            payload["enable_thinking"] = _options.Think;

        if (tools.Count > 0)
            payload["tools"] = tools.Select(ToOpenAiTool).ToList();

        // Coder 400s on json_object ("content field is required") and on tool_choice without tools.
        if (completion?.JsonObjectResponse == true && !isCoder)
            payload["response_format"] = new Dictionary<string, string> { ["type"] = "json_object" };

        using var response = await _httpClient.PostAsJsonAsync(
            "chat/completions",
            payload,
            JsonOptions,
            cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Qwen chat failed. Status={Status} BodyLength={Length}",
                (int)response.StatusCode,
                body.Length);
            var snippet = body.Length <= 300 ? body : body[..300];
            var requestKeys = string.Join(",", payload.Keys);
            throw new InvalidOperationException(
                $"Qwen returned {(int)response.StatusCode}: {snippet} (sent: {requestKeys})");
        }

        var parsed = JsonSerializer.Deserialize<OpenAiChatResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException("Qwen returned an empty chat response.");

        var message = parsed.Choices?.FirstOrDefault()?.Message;
        var toolCalls = ParseToolCalls(message?.ToolCalls);
        return new ChatModelResponse(message?.Content, toolCalls);
    }

    private static Dictionary<string, object?> ToOpenAiMessage(ChatMessage message)
    {
        var dto = new Dictionary<string, object?>
        {
            ["role"] = message.Role,
            ["content"] = message.Content ?? string.Empty
        };

        if (string.Equals(message.Role, "tool", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(message.ToolCallId))
        {
            dto["tool_call_id"] = message.ToolCallId;
        }

        if (message.ToolCalls is { Count: > 0 })
        {
            dto["tool_calls"] = message.ToolCalls.Select(c => new Dictionary<string, object?>
            {
                ["id"] = c.Id,
                ["type"] = "function",
                ["function"] = new Dictionary<string, object?>
                {
                    ["name"] = c.Name,
                    ["arguments"] = c.Arguments.GetRawText()
                }
            }).ToList();
        }

        return dto;
    }

    private static Dictionary<string, object?> ToOpenAiTool(ToolDefinition tool)
        => new()
        {
            ["type"] = "function",
            ["function"] = new Dictionary<string, object?>
            {
                ["name"] = tool.Name,
                ["description"] = tool.Description,
                ["parameters"] = JsonSerializer.Deserialize<JsonElement>(tool.InputSchema.GetRawText())
            }
        };

    private static IReadOnlyList<ToolCall> ParseToolCalls(IReadOnlyList<OpenAiToolCall>? calls)
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

            var id = string.IsNullOrWhiteSpace(call.Id) ? $"call_{index++}" : call.Id;
            result.Add(new ToolCall(id, name, NormalizeArguments(call.Function?.Arguments)));
        }

        return result;
    }

    private static JsonElement NormalizeArguments(string? arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
            return JsonDocument.Parse("{}").RootElement.Clone();

        using var parsed = JsonDocument.Parse(arguments);
        return parsed.RootElement.Clone();
    }

    private sealed class OpenAiChatResponse
    {
        public List<OpenAiChoice>? Choices { get; set; }
    }

    private sealed class OpenAiChoice
    {
        public OpenAiMessage? Message { get; set; }
    }

    private sealed class OpenAiMessage
    {
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OpenAiToolCall>? ToolCalls { get; set; }
    }

    private sealed class OpenAiToolCall
    {
        public string? Id { get; set; }

        public OpenAiFunctionCall? Function { get; set; }
    }

    private sealed class OpenAiFunctionCall
    {
        public string? Name { get; set; }

        public string? Arguments { get; set; }
    }
}
