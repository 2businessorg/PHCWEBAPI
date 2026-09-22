using System.Diagnostics;
using System.Text.Json;
using Agent.Application.Abstractions;
using Agent.Application.Chat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Agent.Application;

/// <summary>
/// Safe tool-call loop: local model → MCP client → local model.
/// The model never talks to MCP directly.
/// </summary>
public sealed class TreasuryAgent : ITreasuryAgent
{
    private readonly ILocalChatModel _chatModel;
    private readonly IMcpClient _mcpClient;
    private readonly AgentOptions _options;
    private readonly ILogger<TreasuryAgent> _logger;

    public TreasuryAgent(
        ILocalChatModel chatModel,
        IMcpClient mcpClient,
        IOptions<AgentOptions> options,
        ILogger<TreasuryAgent> logger)
    {
        _chatModel = chatModel;
        _mcpClient = mcpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AgentChatResult> ChatAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var totalSw = Stopwatch.StartNew();
        _logger.LogInformation("Prompt received. Model={Model}", _chatModel.ModelName);

        var tools = await _mcpClient.ListToolsAsync(cancellationToken);
        var messages = new List<ChatMessage>
        {
            new("system", _options.SystemPrompt),
            new("user", userMessage)
        };

        var toolLogs = new List<AgentToolCallLog>();
        var maxIterations = Math.Max(1, _options.MaxToolIterations);
        var jsonMode = false;
        string? matchCandidatesJson = null;

        for (var iteration = 1; iteration <= maxIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var completion = jsonMode ? ChatCompletionOptions.JsonObject : null;
            var response = await _chatModel.SendAsync(messages, tools, cancellationToken, completion);

            if (!response.HasToolCalls)
            {
                totalSw.Stop();
                _logger.LogInformation(
                    "Agent completed. Model={Model} Iterations={Iterations} Duration={Duration}ms ToolCalls={ToolCalls}",
                    _chatModel.ModelName,
                    iteration,
                    totalSw.ElapsedMilliseconds,
                    toolLogs.Count);

                var answer = response.Content ?? string.Empty;
                if (matchCandidatesJson is not null)
                    answer = ReconciliationMatchAnswerNormalizer.Normalize(answer, matchCandidatesJson);

                return new AgentChatResult(
                    answer,
                    _chatModel.ModelName,
                    iteration,
                    toolLogs);
            }

            messages.Add(new ChatMessage("assistant", response.Content ?? string.Empty, response.ToolCalls));

            foreach (var toolCall in response.ToolCalls)
            {
                ValidateToolCall(toolCall, tools);

                _logger.LogInformation(
                    "Tool selected. Name={Tool} Arguments={Arguments}",
                    toolCall.Name,
                    toolCall.Arguments.GetRawText());

                var toolSw = Stopwatch.StartNew();
                var result = await _mcpClient.CallToolAsync(toolCall.Name, toolCall.Arguments, cancellationToken);
                toolSw.Stop();

                toolLogs.Add(new AgentToolCallLog(
                    toolCall.Name,
                    toolCall.Arguments.GetRawText(),
                    result.IsError,
                    (int)toolSw.ElapsedMilliseconds));

                _logger.LogInformation(
                    "MCP tool executed. Name={Tool} IsError={IsError} Duration={Duration}ms",
                    toolCall.Name,
                    result.IsError,
                    toolSw.ElapsedMilliseconds);

                messages.Add(new ChatMessage(
                    "tool",
                    result.Content,
                    ToolCallId: toolCall.Id));

                if (string.Equals(toolCall.Name, "get_reconciliation_matches", StringComparison.Ordinal))
                {
                    jsonMode = true;
                    if (!result.IsError)
                        matchCandidatesJson = result.Content;
                    messages.Add(new ChatMessage(
                        "user",
                        "Output only a JSON object. No analysis. Schema: " +
                        "{\"combinations\":[{\"status\":\"MATCHED|PARTIAL MATCHED|UNMATCHED\",\"reason\":\"\",\"bankRefs\":[],\"treasuryRefs\":[]}]}. " +
                        "Use party/cheque/invoice/document plus the closer of date or valueDate. " +
                        "MATCHED = same sign + party/cheque/invoice + gap<=30. " +
                        "PARTIAL MATCHED = same sign + party/cheque/invoice + gap>30. " +
                        "UNMATCHED = amount-only or opposite signs. " +
                        "Do not omit 1:1 amount+party pairs. Then 2 leftover UNMATCHED groups."));
                }
            }
        }

        throw new InvalidOperationException(
            $"Agent exceeded MaxToolIterations ({maxIterations}) without a final answer.");
    }

    private static void ValidateToolCall(ToolCall toolCall, IReadOnlyList<ToolDefinition> tools)
    {
        if (string.IsNullOrWhiteSpace(toolCall.Name))
            throw new InvalidOperationException("The model requested a tool without a name.");

        if (tools.All(t => !string.Equals(t.Name, toolCall.Name, StringComparison.Ordinal)))
            throw new InvalidOperationException($"Unknown tool '{toolCall.Name}'.");

        if (toolCall.Arguments.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            throw new InvalidOperationException($"Tool '{toolCall.Name}' was called without JSON arguments.");

        if (toolCall.Arguments.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Tool '{toolCall.Name}' arguments must be a JSON object.");
    }
}
