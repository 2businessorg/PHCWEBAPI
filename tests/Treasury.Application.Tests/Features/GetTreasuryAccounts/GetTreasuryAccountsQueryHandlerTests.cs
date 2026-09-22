using FluentAssertions;
using Moq;
using Treasury.Application.Features.GetTreasuryAccount;
using Treasury.Application.Features.GetTreasuryAccounts;
using Treasury.Domain.Entities;
using Treasury.Domain.Repositories;
using Xunit;

namespace Treasury.Application.Tests.Features.GetTreasuryAccounts;

public class GetTreasuryAccountsQueryHandlerTests
{
    private readonly Mock<ITreasuryAccountRepository> _accounts = new();
    private readonly GetTreasuryAccountsQueryHandler _sut;

    public GetTreasuryAccountsQueryHandlerTests()
    {
        _sut = new GetTreasuryAccountsQueryHandler(_accounts.Object);
    }

    [Fact]
    public async Task Handle_ShouldMapPagedAccounts()
    {
        _accounts
            .Setup(r => r.ListAsync("BCI", false, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, new List<TreasuryAccount>
            {
                new()
                {
                    AccountCode = 22,
                    Name = "BCI",
                    AccountNumber = "123",
                    Currency = "MT",
                    Balance = 1000.50m
                },
                new()
                {
                    AccountCode = 23,
                    Name = "BCI USD",
                    AccountNumber = "456",
                    Currency = "USD",
                    Balance = 10m,
                    Inactive = true
                }
            }));

        var result = await _sut.Handle(
            new GetTreasuryAccountsQuery("BCI", IncludeInactive: false),
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("BCI");
        result.Items[0].Currency.Should().Be("MT");
        result.Items[0].Balance.Should().Be(1000.50m);
        result.Items[1].Inactive.Should().BeTrue();
    }
}

public class GetTreasuryAccountsQueryValidatorTests
{
    private readonly GetTreasuryAccountsQueryValidator _validator = new();

    [Fact]
    public void Validate_PageSizeTooLarge_ShouldFail()
    {
        var result = _validator.Validate(new GetTreasuryAccountsQuery(PageSize: 500));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Default_ShouldPass()
    {
        _validator.Validate(new GetTreasuryAccountsQuery()).IsValid.Should().BeTrue();
    }
}

public class GetTreasuryAccountQueryHandlerTests
{
    [Fact]
    public async Task Handle_UnknownAccount_ShouldThrowKeyNotFound()
    {
        var accounts = new Mock<ITreasuryAccountRepository>();
        accounts
            .Setup(r => r.GetByNameAsync("Inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TreasuryAccount?)null);

        var sut = new GetTreasuryAccountQueryHandler(accounts.Object);

        var act = async () => await sut.Handle(new GetTreasuryAccountQuery("Inexistente"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Inexistente*");
    }

    [Fact]
    public async Task Handle_ExistingAccount_ShouldReturnDto()
    {
        var accounts = new Mock<ITreasuryAccountRepository>();
        accounts
            .Setup(r => r.GetByNameAsync("BCI", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TreasuryAccount
            {
                AccountCode = 22,
                Name = " BCI ",
                AccountNumber = " 99 ",
                Currency = " MT ",
                Balance = 50m
            });

        var sut = new GetTreasuryAccountQueryHandler(accounts.Object);
        var result = await sut.Handle(new GetTreasuryAccountQuery(" BCI "), CancellationToken.None);

        result.Name.Should().Be("BCI");
        result.Code.Should().Be(22);
        result.AccountNumber.Should().Be("99");
        result.Currency.Should().Be("MT");
        result.Balance.Should().Be(50m);
    }
}
