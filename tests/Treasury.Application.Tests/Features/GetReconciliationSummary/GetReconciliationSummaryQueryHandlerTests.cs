using FluentAssertions;
using MediatR;
using Moq;
using Treasury.Application.DTOs;
using Treasury.Application.Features.GetReconciliationMovements;
using Treasury.Application.Features.GetReconciliationSummary;
using Treasury.Application.Mappings;
using Xunit;

namespace Treasury.Application.Tests.Features.GetReconciliationSummary;

public class GetReconciliationSummaryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldProjectMovementsIntoTotalsOnly()
    {
        var movements = new ReconciliationMovementsOutputDTO
        {
            Account = new TreasuryAccountOutputDTO { Name = "BCI", Code = 22, Currency = "MT" },
            Period = new ReconciliationPeriodOutputDTO
            {
                From = new DateOnly(2022, 1, 1),
                To = new DateOnly(2026, 1, 31)
            },
            BankMovements = new[]
            {
                new ImportedBankMovementOutputDTO { Amount = 100m },
                new ImportedBankMovementOutputDTO { Amount = -30m }
            },
            TreasuryMovements = new[]
            {
                new TreasuryAccountMovementOutputDTO { Inflow = 80m, Outflow = 10m }
            },
            BankMovementCount = 2,
            TreasuryMovementCount = 1,
            BankAmountTotal = 70m,
            BankCreditTotal = 100m,
            BankDebitTotal = 30m,
            TreasuryInflowTotal = 80m,
            TreasuryOutflowTotal = 10m,
            TreasuryNetTotal = 70m,
            BankMovementTotalCount = 2,
            TreasuryMovementTotalCount = 1
        };

        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetReconciliationMovementsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(movements);

        var sut = new GetReconciliationSummaryQueryHandler(mediator.Object);
        var result = await sut.Handle(
            new GetReconciliationSummaryQuery("BCI", new DateOnly(2022, 1, 1), new DateOnly(2026, 1, 31)),
            CancellationToken.None);

        result.Account.Name.Should().Be("BCI");
        result.BankMovementCount.Should().Be(2);
        result.TreasuryMovementCount.Should().Be(1);
        result.BankAmountTotal.Should().Be(70m);
        result.BankCreditTotal.Should().Be(100m);
        result.BankDebitTotal.Should().Be(30m);
        result.TreasuryInflowTotal.Should().Be(80m);
        result.TreasuryOutflowTotal.Should().Be(10m);
        result.TreasuryNetTotal.Should().Be(70m);
        result.TotalsAreComplete.Should().BeTrue();
        result.Instruction.Should().Contain("totais");
    }
}

public class ReconciliationTotalsTests
{
    [Fact]
    public void From_ShouldSplitBankCreditsAndDebits()
    {
        var totals = ReconciliationTotals.From(
            new[]
            {
                new ImportedBankMovementOutputDTO { Amount = 10m },
                new ImportedBankMovementOutputDTO { Amount = -4m },
                new ImportedBankMovementOutputDTO { Amount = 2.5m }
            },
            new[]
            {
                new TreasuryAccountMovementOutputDTO { Inflow = 7m, Outflow = 1m },
                new TreasuryAccountMovementOutputDTO { Inflow = 0m, Outflow = 3m }
            });

        totals.BankAmountTotal.Should().Be(8.5m);
        totals.BankCreditTotal.Should().Be(12.5m);
        totals.BankDebitTotal.Should().Be(4m);
        totals.TreasuryInflowTotal.Should().Be(7m);
        totals.TreasuryOutflowTotal.Should().Be(4m);
        totals.TreasuryNetTotal.Should().Be(3m);
    }
}
