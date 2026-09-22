using FluentAssertions;
using Treasury.Application.DTOs;
using Treasury.Application.Matching;
using Xunit;

namespace Treasury.Application.Tests.Matching;

public class ReconciliationMatcherTests
{
    private readonly ReconciliationMatcher _sut = new();

    [Fact]
    public void Match_SameAmountAndCloseDates_ShouldBeMatched()
    {
        var bank = new[]
        {
            Bank("br-1", new DateOnly(2026, 1, 10), 1500m, "PROMAR")
        };
        var treasury = new[]
        {
            Treasury("ba-1", new DateOnly(2026, 1, 12), inflow: 1500m, description: "PROMAR fatura")
        };

        var result = _sut.Match(bank, treasury);

        result.Should().ContainSingle();
        result[0].Status.Should().Be(ReconciliationMatchStatuses.Matched);
        result[0].Bank[0].Id.Should().Be("br-1");
        result[0].Treasury[0].Id.Should().Be("ba-1");
        result[0].DateGapDays.Should().Be(2);
    }

    [Fact]
    public void Match_SameAmountFarDates_ShouldBePartialMatched()
    {
        var bank = new[]
        {
            Bank("br-1", new DateOnly(2023, 1, 10), 10391.86m, "TRF : promar")
        };
        var treasury = new[]
        {
            Treasury("ba-1", new DateOnly(2025, 7, 4), inflow: 10391.86m, description: "PROMAR | 271")
        };

        var result = _sut.Match(bank, treasury);

        result.Should().ContainSingle();
        result[0].Status.Should().Be(ReconciliationMatchStatuses.PartialMatched);
        result[0].Reason.Should().Contain("dia");
    }

    [Fact]
    public void Match_NoAmountOverlap_ShouldBeUnmatchedOnBothSides()
    {
        var bank = new[]
        {
            Bank("br-1", new DateOnly(2022, 12, 2), 19987.11m, "ECLIPSE")
        };
        var treasury = new[]
        {
            Treasury("ba-1", new DateOnly(2025, 6, 11), outflow: 600m, description: "Telecom")
        };

        var result = _sut.Match(bank, treasury);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Status == ReconciliationMatchStatuses.Unmatched);
        result.Should().Contain(c => c.BankCount == 1 && c.TreasuryCount == 0);
        result.Should().Contain(c => c.BankCount == 0 && c.TreasuryCount == 1);
    }

    [Fact]
    public void Match_OneBankEqualsTwoTreasury_ShouldCombine()
    {
        var bank = new[]
        {
            Bank("br-1", new DateOnly(2026, 1, 10), -150m, "pagamento")
        };
        var treasury = new[]
        {
            Treasury("ba-1", new DateOnly(2026, 1, 10), outflow: 100m, description: "parcela 1"),
            Treasury("ba-2", new DateOnly(2026, 1, 11), outflow: 50m, description: "parcela 2")
        };

        var result = _sut.Match(bank, treasury);

        result.Should().ContainSingle();
        result[0].Status.Should().Be(ReconciliationMatchStatuses.Matched);
        result[0].BankCount.Should().Be(1);
        result[0].TreasuryCount.Should().Be(2);
        result[0].BankAmountTotal.Should().Be(-150m);
        result[0].TreasuryAmountTotal.Should().Be(-150m);
    }

    [Fact]
    public void Match_ShouldNotReuseTheSameTreasuryLine()
    {
        var bank = new[]
        {
            Bank("br-1", new DateOnly(2026, 1, 10), 100m, "A"),
            Bank("br-2", new DateOnly(2026, 1, 10), 100m, "B")
        };
        var treasury = new[]
        {
            Treasury("ba-1", new DateOnly(2026, 1, 10), inflow: 100m, description: "A")
        };

        var result = _sut.Match(bank, treasury);

        result.Count(c => c.Status == ReconciliationMatchStatuses.Matched).Should().Be(1);
        result.Count(c => c.Status == ReconciliationMatchStatuses.Unmatched).Should().Be(1);
        result.SelectMany(c => c.Treasury).Count(l => l.Id == "ba-1").Should().Be(1);
    }

    private static ImportedBankMovementOutputDTO Bank(
        string id,
        DateOnly date,
        decimal amount,
        string description)
        => new()
        {
            Id = id,
            Date = date,
            ValueDate = date,
            Amount = amount,
            Description = description
        };

    private static TreasuryAccountMovementOutputDTO Treasury(
        string id,
        DateOnly date,
        decimal inflow = 0,
        decimal outflow = 0,
        string description = "")
        => new()
        {
            Id = id,
            Date = date,
            ValueDate = date,
            Inflow = inflow,
            Outflow = outflow,
            Description = description
        };
}
