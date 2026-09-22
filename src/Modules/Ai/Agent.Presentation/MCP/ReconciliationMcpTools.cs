using System.ComponentModel;
using System.Text.Json;
using Agent.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Treasury.Application.Features.GetReconciliationMovements;
using Treasury.Application.Features.GetReconciliationSummary;

namespace Agent.Presentation.MCP;

/// <summary>
/// Read-only MCP tool: compact BR/BA movements (totals + a few sample lines).
/// </summary>
[McpServerToolType]
public sealed class ReconciliationMcpTools : IMcpToolHandler
{
    public const string ToolName = "get_reconciliation_movements";

    public const string ToolDescription =
        "Returns a compact reconciliation view for a PHC treasury account and date range: " +
        "precomputed counts, money totals, and a few sample lines. " +
        "Use this when the user wants movements or examples. Prefer get_reconciliation_summary for totals only. " +
        "accountName must be the exact bl.banco name (example: BCI, CX). Dates must be YYYY-MM-DD.";

    private static readonly JsonElement Schema = JsonDocument.Parse(
        """
        {
          "type": "object",
          "properties": {
            "accountName": {
              "type": "string",
              "description": "Exact PHC treasury account name (bl.banco), e.g. BCI or CX."
            },
            "dateFrom": {
              "type": "string",
              "description": "Start date in YYYY-MM-DD format."
            },
            "dateTo": {
              "type": "string",
              "description": "End date in YYYY-MM-DD format."
            }
          },
          "required": ["accountName", "dateFrom", "dateTo"]
        }
        """).RootElement.Clone();

    private readonly IMediator _mediator;
    private readonly ILogger<ReconciliationMcpTools> _logger;

    public ReconciliationMcpTools(IMediator mediator, ILogger<ReconciliationMcpTools> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public string Name => ToolName;

    public string Description => ToolDescription;

    public JsonElement InputSchema => Schema;

    [McpServerTool(Name = ToolName), Description(ToolDescription)]
    public Task<string> GetReconciliationMovements(
        [Description("Exact PHC treasury account name (bl.banco), e.g. BCI or CX.")] string accountName,
        [Description("Start date in YYYY-MM-DD format.")] string dateFrom,
        [Description("End date in YYYY-MM-DD format.")] string dateTo,
        CancellationToken cancellationToken)
    {
        var args = JsonSerializer.SerializeToElement(new
        {
            accountName,
            dateFrom,
            dateTo
        }, McpToolJson.Options);

        return InvokeAsync(args, cancellationToken);
    }

    public async Task<string> InvokeAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var input = ParseArguments(arguments);

        _logger.LogInformation(
            "Reconciliation MCP tool executing. Account={Account} Period={From}..{To}",
            input.AccountName,
            input.DateFrom,
            input.DateTo);

        var result = await _mediator.Send(
            new GetReconciliationMovementsQuery(input.AccountName, input.DateFrom, input.DateTo),
            cancellationToken);

        _logger.LogInformation(
            "Reconciliation MCP tool executed. Account={Account} Period={From}..{To} BRCount={BRCount} BACount={BACount}",
            result.Account.Name,
            result.Period.From,
            result.Period.To,
            result.BankMovementCount,
            result.TreasuryMovementCount);

        return McpToolJson.Serialize(ReconciliationMcpCompact.FromMovements(result));
    }

    public static ReconciliationToolArguments ParseArguments(JsonElement arguments)
    {
        if (arguments.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Tool arguments must be a JSON object.");

        var accountName = McpToolJson.ReadRequiredString(arguments, "accountName");
        var dateFrom = McpToolJson.ReadRequiredDate(arguments, "dateFrom");
        var dateTo = McpToolJson.ReadRequiredDate(arguments, "dateTo");

        if (dateFrom > dateTo)
            throw new ArgumentException("dateFrom must be less than or equal to dateTo.");

        return new ReconciliationToolArguments(accountName, dateFrom, dateTo);
    }
}

/// <summary>
/// Read-only MCP tool: counts and money totals only, no line dump.
/// </summary>
[McpServerToolType]
public sealed class ReconciliationSummaryMcpTool : IMcpToolHandler
{
    public const string ToolName = "get_reconciliation_summary";

    public const string ToolDescription =
        "Returns only counts and precomputed money totals for unreconciled bank (BR) and treasury (BA) movements. " +
        "Prefer this tool for questions about how many movements or total amounts. " +
        "accountName must be the exact bl.banco name. Dates must be YYYY-MM-DD. Do not sum anything yourself.";

    private static readonly JsonElement Schema = JsonDocument.Parse(
        """
        {
          "type": "object",
          "properties": {
            "accountName": {
              "type": "string",
              "description": "Exact PHC treasury account name (bl.banco), e.g. BCI or CX."
            },
            "dateFrom": {
              "type": "string",
              "description": "Start date in YYYY-MM-DD format."
            },
            "dateTo": {
              "type": "string",
              "description": "End date in YYYY-MM-DD format."
            }
          },
          "required": ["accountName", "dateFrom", "dateTo"]
        }
        """).RootElement.Clone();

    private readonly IMediator _mediator;
    private readonly ILogger<ReconciliationSummaryMcpTool> _logger;

    public ReconciliationSummaryMcpTool(IMediator mediator, ILogger<ReconciliationSummaryMcpTool> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public string Name => ToolName;

    public string Description => ToolDescription;

    public JsonElement InputSchema => Schema;

    [McpServerTool(Name = ToolName), Description(ToolDescription)]
    public Task<string> GetReconciliationSummary(
        [Description("Exact PHC treasury account name (bl.banco), e.g. BCI or CX.")] string accountName,
        [Description("Start date in YYYY-MM-DD format.")] string dateFrom,
        [Description("End date in YYYY-MM-DD format.")] string dateTo,
        CancellationToken cancellationToken)
    {
        var args = JsonSerializer.SerializeToElement(new
        {
            accountName,
            dateFrom,
            dateTo
        }, McpToolJson.Options);

        return InvokeAsync(args, cancellationToken);
    }

    public async Task<string> InvokeAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var input = ReconciliationMcpTools.ParseArguments(arguments);

        _logger.LogInformation(
            "Reconciliation summary MCP tool executing. Account={Account} Period={From}..{To}",
            input.AccountName,
            input.DateFrom,
            input.DateTo);

        var result = await _mediator.Send(
            new GetReconciliationSummaryQuery(input.AccountName, input.DateFrom, input.DateTo),
            cancellationToken);

        return McpToolJson.Serialize(ReconciliationMcpCompact.FromSummary(result));
    }
}

/// <summary>
/// MCP tool that returns numbered BR/BA lines so the model can propose matches itself.
/// </summary>
[McpServerToolType]
public sealed class ReconciliationMatchesMcpTool : IMcpToolHandler
{
    public const string ToolName = "get_reconciliation_matches";

    public const string ToolDescription =
        "Loads numbered unreconciled bank (B1..) and treasury (T1..) lines. Does not decide MATCHED. " +
        "Call once when the user asks to reconcile, match, combine movements, or wants MATCHED/UNMATCHED JSON. " +
        "Then YOU classify using party, cheque, document, invoice, date and valueDate. " +
        "MATCHED is allowed only when signed amount + (party or cheque or invoice) + |date/valueDate gap|<=30 days are ALL true. " +
        "If amount+party match but |date gap|>30 days, status MUST be PARTIAL MATCHED (MATCHED is forbidden). " +
        "UNMATCHED = amount-only (different names), opposite signs (+X vs -X), or no pair. " +
        "Do not match opposite signs. Do not dump chain-of-thought. Reply with JSON only. " +
        "1:N/N:1 only when signed amounts sum and descriptions support it. Do not call this tool twice. Dates YYYY-MM-DD.";

    private static readonly JsonElement Schema = JsonDocument.Parse(
        """
        {
          "type": "object",
          "properties": {
            "accountName": {
              "type": "string",
              "description": "Exact PHC treasury account name (bl.banco), e.g. BCI or CX."
            },
            "dateFrom": {
              "type": "string",
              "description": "Start date in YYYY-MM-DD format."
            },
            "dateTo": {
              "type": "string",
              "description": "End date in YYYY-MM-DD format."
            }
          },
          "required": ["accountName", "dateFrom", "dateTo"]
        }
        """).RootElement.Clone();

    private readonly IMediator _mediator;
    private readonly ILogger<ReconciliationMatchesMcpTool> _logger;

    public ReconciliationMatchesMcpTool(IMediator mediator, ILogger<ReconciliationMatchesMcpTool> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public string Name => ToolName;

    public string Description => ToolDescription;

    public JsonElement InputSchema => Schema;

    [McpServerTool(Name = ToolName), Description(ToolDescription)]
    public Task<string> GetReconciliationMatches(
        [Description("Exact PHC treasury account name (bl.banco), e.g. BCI or CX.")] string accountName,
        [Description("Start date in YYYY-MM-DD format.")] string dateFrom,
        [Description("End date in YYYY-MM-DD format.")] string dateTo,
        CancellationToken cancellationToken)
    {
        var args = JsonSerializer.SerializeToElement(new
        {
            accountName,
            dateFrom,
            dateTo
        }, McpToolJson.Options);

        return InvokeAsync(args, cancellationToken);
    }

    public async Task<string> InvokeAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var input = ReconciliationMcpTools.ParseArguments(arguments);

        _logger.LogInformation(
            "Reconciliation matches MCP tool executing. Account={Account} Period={From}..{To}",
            input.AccountName,
            input.DateFrom,
            input.DateTo);

        var result = await _mediator.Send(
            new GetReconciliationMovementsQuery(input.AccountName, input.DateFrom, input.DateTo),
            cancellationToken);

        _logger.LogInformation(
            "Reconciliation match candidates loaded. Account={Account} BRCount={BRCount} BACount={BACount}",
            result.Account.Name,
            result.BankMovementCount,
            result.TreasuryMovementCount);

        return McpToolJson.Serialize(ReconciliationMcpCompact.FromMatchCandidates(result));
    }
}

/// <summary>
/// Parsed MCP tool arguments for reconciliation period queries.
/// </summary>
public readonly record struct ReconciliationToolArguments(string AccountName, DateOnly DateFrom, DateOnly DateTo);
