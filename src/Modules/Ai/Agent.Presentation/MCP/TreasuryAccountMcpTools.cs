using System.ComponentModel;
using System.Text.Json;
using Agent.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Treasury.Application.Features.GetTreasuryAccount;
using Treasury.Application.Features.GetTreasuryAccounts;

namespace Agent.Presentation.MCP;

/// <summary>
/// MCP tool to discover PHC treasury account names (<c>bl.banco</c>).
/// </summary>
[McpServerToolType]
public sealed class ListTreasuryAccountsMcpTool : IMcpToolHandler
{
    public const string ToolName = "list_treasury_accounts";

    public const string ToolDescription =
        "Lists PHC treasury accounts with name, code, account number, currency and balance. " +
        "Call this first when the user does not give an exact account name, or to confirm that an account exists. " +
        "Optional nameContains filters by name fragment. Use the returned name exactly as accountName in other tools.";

    private static readonly JsonElement Schema = JsonDocument.Parse(
        """
        {
          "type": "object",
          "properties": {
            "nameContains": {
              "type": "string",
              "description": "Optional name fragment to filter accounts, e.g. BCI or Caixa."
            },
            "includeInactive": {
              "type": "boolean",
              "description": "Include inactive accounts. Default false."
            }
          }
        }
        """).RootElement.Clone();

    private readonly IMediator _mediator;
    private readonly ILogger<ListTreasuryAccountsMcpTool> _logger;

    public ListTreasuryAccountsMcpTool(IMediator mediator, ILogger<ListTreasuryAccountsMcpTool> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public string Name => ToolName;

    public string Description => ToolDescription;

    public JsonElement InputSchema => Schema;

    [McpServerTool(Name = ToolName), Description(ToolDescription)]
    public Task<string> ListTreasuryAccounts(
        [Description("Optional name fragment to filter accounts, e.g. BCI or Caixa.")] string? nameContains = null,
        [Description("Include inactive accounts. Default false.")] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var args = JsonSerializer.SerializeToElement(new
        {
            nameContains,
            includeInactive
        }, McpToolJson.Options);

        return InvokeAsync(args, cancellationToken);
    }

    public async Task<string> InvokeAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var nameContains = arguments.ValueKind == JsonValueKind.Object
            ? McpToolJson.ReadOptionalString(arguments, "nameContains")
            : null;
        var includeInactive = arguments.ValueKind == JsonValueKind.Object
            && McpToolJson.ReadOptionalBool(arguments, "includeInactive");

        _logger.LogInformation(
            "List treasury accounts MCP tool executing. Filter={Filter} IncludeInactive={IncludeInactive}",
            nameContains,
            includeInactive);

        var result = await _mediator.Send(
            new GetTreasuryAccountsQuery(nameContains, includeInactive, Page: 1, PageSize: 50),
            cancellationToken);

        return McpToolJson.Serialize(ReconciliationMcpCompact.FromAccounts(result));
    }
}

/// <summary>
/// MCP tool to load one treasury account by exact name.
/// </summary>
[McpServerToolType]
public sealed class GetTreasuryAccountMcpTool : IMcpToolHandler
{
    public const string ToolName = "get_treasury_account";

    public const string ToolDescription =
        "Returns one PHC treasury account by exact name (bl.banco): code, account number, currency, balance and inactive flag. " +
        "Use list_treasury_accounts first if the exact name is unknown.";

    private static readonly JsonElement Schema = JsonDocument.Parse(
        """
        {
          "type": "object",
          "properties": {
            "accountName": {
              "type": "string",
              "description": "Exact PHC treasury account name (bl.banco), e.g. BCI or CX."
            }
          },
          "required": ["accountName"]
        }
        """).RootElement.Clone();

    private readonly IMediator _mediator;
    private readonly ILogger<GetTreasuryAccountMcpTool> _logger;

    public GetTreasuryAccountMcpTool(IMediator mediator, ILogger<GetTreasuryAccountMcpTool> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public string Name => ToolName;

    public string Description => ToolDescription;

    public JsonElement InputSchema => Schema;

    [McpServerTool(Name = ToolName), Description(ToolDescription)]
    public Task<string> GetTreasuryAccount(
        [Description("Exact PHC treasury account name (bl.banco), e.g. BCI or CX.")] string accountName,
        CancellationToken cancellationToken)
    {
        var args = JsonSerializer.SerializeToElement(new { accountName }, McpToolJson.Options);
        return InvokeAsync(args, cancellationToken);
    }

    public async Task<string> InvokeAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        if (arguments.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Tool arguments must be a JSON object.");

        var accountName = McpToolJson.ReadRequiredString(arguments, "accountName");

        _logger.LogInformation("Get treasury account MCP tool executing. Account={Account}", accountName);

        var result = await _mediator.Send(new GetTreasuryAccountQuery(accountName), cancellationToken);
        return McpToolJson.Serialize(ReconciliationMcpCompact.FromAccount(result));
    }
}
