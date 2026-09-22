using System.Text.Json;
using Agent.Application;
using Agent.Application.Abstractions;
using Agent.Application.Chat;
using Agent.Presentation.MCP;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Agent.Application.Tests.Features.SendAgentChat;

public class ReconciliationMcpToolsArgumentTests
{
    [Fact]
    public void ParseArguments_ValidPayload_ShouldMapToUseCaseInputs()
    {
        using var doc = JsonDocument.Parse(
            """
            {
              "accountName": "Caixa Sócios",
              "dateFrom": "2026-01-01",
              "dateTo": "2026-02-28"
            }
            """);

        var parsed = ReconciliationMcpTools.ParseArguments(doc.RootElement);

        parsed.AccountName.Should().Be("Caixa Sócios");
        parsed.DateFrom.Should().Be(new DateOnly(2026, 1, 1));
        parsed.DateTo.Should().Be(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public void ParseArguments_InvalidDate_ShouldThrow()
    {
        using var doc = JsonDocument.Parse(
            """
            {
              "accountName": "Caixa Sócios",
              "dateFrom": "01/01/2026",
              "dateTo": "2026-02-28"
            }
            """);

        var act = () => ReconciliationMcpTools.ParseArguments(doc.RootElement);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*YYYY-MM-DD*");
    }

    [Fact]
    public void ParseArguments_DateFromAfterDateTo_ShouldThrow()
    {
        using var doc = JsonDocument.Parse(
            """
            {
              "accountName": "Caixa Sócios",
              "dateFrom": "2026-03-01",
              "dateTo": "2026-02-01"
            }
            """);

        var act = () => ReconciliationMcpTools.ParseArguments(doc.RootElement);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*dateFrom*");
    }
}

public class TreasuryAgentTests
{
    [Fact]
    public async Task ChatAsync_ModelRequestsTool_ShouldCallMcpAndReturnFinalAnswer()
    {
        var chatModel = new Mock<ILocalChatModel>();
        chatModel.SetupGet(m => m.ModelName).Returns("qwen2.5-coder:1.5b");

        var mcp = new Mock<IMcpClient>();
        using var schema = JsonDocument.Parse("""{"type":"object"}""");
        mcp.Setup(c => c.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ToolDefinition>
            {
                new("get_reconciliation_movements", "desc", schema.RootElement.Clone())
            });

        using var args = JsonDocument.Parse(
            """
            {
              "accountName": "Caixa Sócios",
              "dateFrom": "2026-01-01",
              "dateTo": "2026-02-28"
            }
            """);

        chatModel
            .SetupSequence(m => m.SendAsync(
                It.IsAny<IReadOnlyCollection<ChatMessage>>(),
                It.IsAny<IReadOnlyCollection<ToolDefinition>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ChatCompletionOptions?>()))
            .ReturnsAsync(new ChatModelResponse(null, new List<ToolCall>
            {
                new("call_0", "get_reconciliation_movements", args.RootElement.Clone())
            }))
            .ReturnsAsync(new ChatModelResponse("31 BR and 57 BA movements.", Array.Empty<ToolCall>()));

        mcp.Setup(c => c.CallToolAsync(
                "get_reconciliation_movements",
                It.IsAny<JsonElement>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new McpToolResult(false, """{"bankMovementCount":31,"treasuryMovementCount":57}"""));

        var agent = new TreasuryAgent(
            chatModel.Object,
            mcp.Object,
            Options.Create(new AgentOptions { MaxToolIterations = 10 }),
            Mock.Of<ILogger<TreasuryAgent>>());

        var result = await agent.ChatAsync(
            "Peço a lista de BR e BA para a conta de nome Caixa Sócios com datas entre janeiro e fevereiro de 2026.");

        result.Answer.Should().Be("31 BR and 57 BA movements.");
        result.Iterations.Should().Be(2);
        result.ToolCalls.Should().ContainSingle(t => t.Name == "get_reconciliation_movements" && !t.IsError);
        mcp.Verify(c => c.CallToolAsync(
            "get_reconciliation_movements",
            It.IsAny<JsonElement>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChatAsync_UnknownTool_ShouldThrow()
    {
        var chatModel = new Mock<ILocalChatModel>();
        chatModel.SetupGet(m => m.ModelName).Returns("qwen2.5-coder:1.5b");

        var mcp = new Mock<IMcpClient>();
        using var schema = JsonDocument.Parse("""{"type":"object"}""");
        mcp.Setup(c => c.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ToolDefinition>
            {
                new("get_reconciliation_movements", "desc", schema.RootElement.Clone())
            });

        using var args = JsonDocument.Parse("""{"accountName":"x"}""");
        chatModel
            .Setup(m => m.SendAsync(
                It.IsAny<IReadOnlyCollection<ChatMessage>>(),
                It.IsAny<IReadOnlyCollection<ToolDefinition>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ChatCompletionOptions?>()))
            .ReturnsAsync(new ChatModelResponse(null, new List<ToolCall>
            {
                new("call_0", "delete_everything", args.RootElement.Clone())
            }));

        var agent = new TreasuryAgent(
            chatModel.Object,
            mcp.Object,
            Options.Create(new AgentOptions { MaxToolIterations = 3 }),
            Mock.Of<ILogger<TreasuryAgent>>());

        var act = async () => await agent.ChatAsync("apaga tudo");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Unknown tool*");
    }

    [Fact]
    public async Task ChatAsync_MatchTool_ShouldLetTheModelProduceTheFinalAnswer()
    {
        var chatModel = new Mock<ILocalChatModel>();
        chatModel.SetupGet(m => m.ModelName).Returns("ibm/granite4:1b");

        var mcp = new Mock<IMcpClient>();
        using var schema = JsonDocument.Parse("""{"type":"object"}""");
        mcp.Setup(c => c.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ToolDefinition>
            {
                new("get_reconciliation_matches", "desc", schema.RootElement.Clone())
            });

        using var args = JsonDocument.Parse(
            """
            {
              "accountName": "BCI",
              "dateFrom": "2022-01-01",
              "dateTo": "2026-01-31"
            }
            """);

        const string modelJson = """{"combinations":[{"status":"MATCHED","bankRefs":["B1"],"treasuryRefs":["T1","T2"]}]}""";
        chatModel
            .SetupSequence(m => m.SendAsync(
                It.IsAny<IReadOnlyCollection<ChatMessage>>(),
                It.IsAny<IReadOnlyCollection<ToolDefinition>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ChatCompletionOptions?>()))
            .ReturnsAsync(new ChatModelResponse(null, new List<ToolCall>
            {
                new("call_0", "get_reconciliation_matches", args.RootElement.Clone())
            }))
            .ReturnsAsync(new ChatModelResponse(modelJson, Array.Empty<ToolCall>()));

        mcp.Setup(c => c.CallToolAsync(
                "get_reconciliation_matches",
                It.IsAny<JsonElement>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new McpToolResult(false, """{"bankMovements":[{"refId":"B1","amount":1000}],"treasuryMovements":[{"refId":"T1","amount":600},{"refId":"T2","amount":400}]}"""));

        var agent = new TreasuryAgent(
            chatModel.Object,
            mcp.Object,
            Options.Create(new AgentOptions { MaxToolIterations = 10 }),
            Mock.Of<ILogger<TreasuryAgent>>());

        var result = await agent.ChatAsync(
            "mande um json com MATCHED|PARTIAL MATCHED|UNMATCHED da BCI");

        using var answer = JsonDocument.Parse(result.Answer);
        answer.RootElement.GetProperty("combinations")[0].GetProperty("status").GetString().Should().Be("MATCHED");
        result.Iterations.Should().Be(2);
        chatModel.Verify(
            m => m.SendAsync(
                It.IsAny<IReadOnlyCollection<ChatMessage>>(),
                It.Is<IReadOnlyCollection<ToolDefinition>>(t => t.Count > 0),
                It.IsAny<CancellationToken>(),
                It.Is<ChatCompletionOptions?>(o => o == null)),
            Times.Once);
        chatModel.Verify(
            m => m.SendAsync(
                It.IsAny<IReadOnlyCollection<ChatMessage>>(),
                It.Is<IReadOnlyCollection<ToolDefinition>>(t => t.Count > 0),
                It.IsAny<CancellationToken>(),
                It.Is<ChatCompletionOptions?>(o => o != null && o.JsonObjectResponse)),
            Times.Once);
    }
}
