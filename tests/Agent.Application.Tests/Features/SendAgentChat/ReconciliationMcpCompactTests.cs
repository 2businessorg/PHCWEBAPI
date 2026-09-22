using System.Text.Json;
using Agent.Presentation.MCP;
using FluentAssertions;
using Treasury.Application.DTOs;
using Xunit;

namespace Agent.Application.Tests.Features.SendAgentChat;

public class ReconciliationMcpCompactTests
{
    [Fact]
    public void FromMovements_ShouldExposeTotalsAndOnlySampleLines()
    {
        var movements = new ReconciliationMovementsOutputDTO
        {
            Account = new TreasuryAccountOutputDTO
            {
                Name = "BCI",
                Code = 22,
                Currency = "MT",
                Balance = 10m
            },
            Period = new ReconciliationPeriodOutputDTO
            {
                From = new DateOnly(2022, 1, 1),
                To = new DateOnly(2026, 1, 31)
            },
            BankMovements = Enumerable.Range(1, 8)
                .Select(i => new ImportedBankMovementOutputDTO
                {
                    Id = $"stamp-{i}",
                    Date = new DateOnly(2022, 1, i),
                    Description = $"bank {i}",
                    Amount = i
                })
                .ToList(),
            TreasuryMovements = Enumerable.Range(1, 3)
                .Select(i => new TreasuryAccountMovementOutputDTO
                {
                    Id = $"ba-{i}",
                    Date = new DateOnly(2022, 1, i),
                    Description = $"ba {i}",
                    Inflow = i * 10m
                })
                .ToList(),
            BankMovementCount = 8,
            TreasuryMovementCount = 3,
            BankAmountTotal = 36m,
            BankCreditTotal = 36m,
            TreasuryInflowTotal = 60m,
            TreasuryNetTotal = 60m,
            BankMovementTotalCount = 8,
            TreasuryMovementTotalCount = 3
        };

        var json = JsonSerializer.Serialize(
            ReconciliationMcpCompact.FromMovements(movements),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("summary").GetProperty("bankMovementCount").GetInt32().Should().Be(8);
        root.GetProperty("summary").GetProperty("bankAmountTotal").GetDecimal().Should().Be(36m);
        root.GetProperty("sampleBankMovements").GetArrayLength().Should().Be(5);
        root.GetProperty("sampleTreasuryMovements").GetArrayLength().Should().Be(3);
        root.GetProperty("truncated").GetBoolean().Should().BeTrue();
        json.Should().NotContain("stamp-6");
        json.Should().Contain("Usa os totais");
    }

    [Fact]
    public void FromMatchCandidates_ShouldNumberLinesAndUseSignedTreasuryAmount()
    {
        var movements = new ReconciliationMovementsOutputDTO
        {
            Account = new TreasuryAccountOutputDTO { Name = "BCI" },
            Period = new ReconciliationPeriodOutputDTO
            {
                From = new DateOnly(2026, 1, 1),
                To = new DateOnly(2026, 1, 31)
            },
            BankMovements = new[]
            {
                new ImportedBankMovementOutputDTO
                {
                    Date = new DateOnly(2026, 1, 10),
                    ValueDate = new DateOnly(2026, 1, 11),
                    Amount = 1000m,
                    Description = "TRF : 2Business-FT3/2023 - GEOSYSTEMS",
                    Document = "FT3/2023",
                    Cheque = "99"
                }
            },
            TreasuryMovements = new[]
            {
                new TreasuryAccountMovementOutputDTO
                {
                    Date = new DateOnly(2026, 1, 10),
                    Inflow = 0,
                    Outflow = 600m,
                    Description = "parcela 1"
                },
                new TreasuryAccountMovementOutputDTO
                {
                    Date = new DateOnly(2026, 1, 11),
                    Inflow = 0,
                    Outflow = 400m,
                    Description = "parcela 2"
                }
            },
            BankMovementTotalCount = 1,
            TreasuryMovementTotalCount = 2
        };

        var json = JsonSerializer.Serialize(
            ReconciliationMcpCompact.FromMatchCandidates(movements),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var bank = root.GetProperty("bankMovements")[0];
        var t1 = root.GetProperty("treasuryMovements")[0];
        var t2 = root.GetProperty("treasuryMovements")[1];

        bank.GetProperty("refId").GetString().Should().Be("B1");
        bank.GetProperty("amount").GetDecimal().Should().Be(1000m);
        bank.GetProperty("valueDate").GetString().Should().Be("2026-01-11");
        bank.GetProperty("party").GetString().Should().Contain("GEOSYSTEMS");
        bank.GetProperty("invoice").GetString().Should().Contain("FT");
        t1.GetProperty("refId").GetString().Should().Be("T1");
        t1.GetProperty("amount").GetDecimal().Should().Be(-600m);
        t2.GetProperty("amount").GetDecimal().Should().Be(-400m);
        json.Should().Contain("TU fazes o match");
        json.Should().Contain("PARTIAL MATCHED");
        json.Should().Contain("party");
    }
}
