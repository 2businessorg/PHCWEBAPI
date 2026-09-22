using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Treasury.Application.Features.GetReconciliationMovements;
using Treasury.Domain.Entities;
using Treasury.Domain.Repositories;
using Xunit;

namespace Treasury.Application.Tests.Features.GetReconciliationMovements;

public class GetReconciliationMovementsQueryHandlerTests
{
    private readonly Mock<ITreasuryAccountRepository> _accounts = new();
    private readonly Mock<IBankReconciliationRepository> _movements = new();
    private readonly GetReconciliationMovementsQueryHandler _sut;

    public GetReconciliationMovementsQueryHandlerTests()
    {
        _sut = new GetReconciliationMovementsQueryHandler(
            _accounts.Object,
            _movements.Object,
            Mock.Of<ILogger<GetReconciliationMovementsQueryHandler>>());
    }

    [Fact]
    public async Task Handle_ValidAccount_ShouldReturnMappedMovements()
    {
        var account = new TreasuryAccount
        {
            AccountCode = 21,
            Name = "Caixa Sócios",
            AccountNumber = "CXSOC"
        };

        _accounts
            .Setup(r => r.GetByNameAsync("Caixa Sócios", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _movements
            .Setup(r => r.GetImportedBankMovementsAsync(21, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync((1, new List<ImportedBankMovement>
            {
                new()
                {
                    Id = "br-1",
                    Date = new DateTime(2026, 1, 6),
                    ValueDate = new DateTime(2026, 1, 6),
                    Description = "Deposito cheque",
                    Amount = 46048.52m,
                    Document = "",
                    Cheque = "7415389"
                }
            }));

        _movements
            .Setup(r => r.GetTreasuryAccountMovementsAsync(21, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync((1, new List<TreasuryAccountMovement>
            {
                new()
                {
                    Id = "ba-1",
                    Date = new DateTime(2026, 1, 10),
                    ValueDate = new DateTime(2026, 1, 10),
                    Document = "Emprest. de Sócios",
                    Description = "Crédito de sócios",
                    Inflow = 2311.01m,
                    Outflow = 0,
                    Cheque = ""
                }
            }));

        var query = new GetReconciliationMovementsQuery(
            "Caixa Sócios",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 28));

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Account.Name.Should().Be("Caixa Sócios");
        result.Account.Code.Should().Be(21);
        result.Period.From.Should().Be(new DateOnly(2026, 1, 1));
        result.Period.To.Should().Be(new DateOnly(2026, 2, 28));
        result.BankMovementCount.Should().Be(1);
        result.TreasuryMovementCount.Should().Be(1);
        result.BankMovements[0].Amount.Should().Be(46048.52m);
        result.TreasuryMovements[0].Inflow.Should().Be(2311.01m);
        result.TreasuryMovements[0].Outflow.Should().Be(0);
        result.BankAmountTotal.Should().Be(46048.52m);
        result.BankCreditTotal.Should().Be(46048.52m);
        result.BankDebitTotal.Should().Be(0);
        result.TreasuryInflowTotal.Should().Be(2311.01m);
        result.TreasuryOutflowTotal.Should().Be(0);
        result.TreasuryNetTotal.Should().Be(2311.01m);
    }

    [Fact]
    public async Task Handle_UnknownAccount_ShouldThrowKeyNotFound()
    {
        _accounts
            .Setup(r => r.GetByNameAsync("Inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TreasuryAccount?)null);

        var query = new GetReconciliationMovementsQuery(
            "Inexistente",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 28));

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Inexistente*");
    }

    [Fact]
    public async Task Handle_NoMovements_ShouldReturnEmptyCollections()
    {
        _accounts
            .Setup(r => r.GetByNameAsync("Caixa Sócios", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TreasuryAccount { AccountCode = 21, Name = "Caixa Sócios" });

        _movements
            .Setup(r => r.GetImportedBankMovementsAsync(21, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, Array.Empty<ImportedBankMovement>()));

        _movements
            .Setup(r => r.GetTreasuryAccountMovementsAsync(21, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, Array.Empty<TreasuryAccountMovement>()));

        var result = await _sut.Handle(
            new GetReconciliationMovementsQuery("Caixa Sócios", new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 28)),
            CancellationToken.None);

        result.BankMovements.Should().BeEmpty();
        result.TreasuryMovements.Should().BeEmpty();
        result.BankMovementCount.Should().Be(0);
        result.TreasuryMovementCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldUseInclusiveDateWindow()
    {
        _accounts
            .Setup(r => r.GetByNameAsync("Caixa Sócios", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TreasuryAccount { AccountCode = 21, Name = "Caixa Sócios" });

        _movements
            .Setup(r => r.GetImportedBankMovementsAsync(21, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, Array.Empty<ImportedBankMovement>()));

        _movements
            .Setup(r => r.GetTreasuryAccountMovementsAsync(21, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, Array.Empty<TreasuryAccountMovement>()));

        await _sut.Handle(
            new GetReconciliationMovementsQuery("Caixa Sócios", new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 28)),
            CancellationToken.None);

        _movements.Verify(r => r.GetImportedBankMovementsAsync(
            21,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 3, 1),
            1,
            500,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class GetReconciliationMovementsQueryValidatorTests
{
    private readonly GetReconciliationMovementsQueryValidator _validator = new();

    [Fact]
    public void Validate_DateFromAfterDateTo_ShouldFail()
    {
        var result = _validator.Validate(new GetReconciliationMovementsQuery(
            "Caixa Sócios",
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 2, 1)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("dateFrom"));
    }

    [Fact]
    public void Validate_EmptyAccountName_ShouldFail()
    {
        var result = _validator.Validate(new GetReconciliationMovementsQuery(
            "",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 28)));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ValidRange_ShouldPass()
    {
        var result = _validator.Validate(new GetReconciliationMovementsQuery(
            "Caixa Sócios",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 28)));

        result.IsValid.Should().BeTrue();
    }
}
