using FluentValidation;

namespace Treasury.Application.Features.GetTreasuryAccounts;

/// <summary>
/// Validates <see cref="GetTreasuryAccountsQuery"/>.
/// </summary>
public class GetTreasuryAccountsQueryValidator : AbstractValidator<GetTreasuryAccountsQuery>
{
    public GetTreasuryAccountsQueryValidator()
    {
        RuleFor(x => x.NameContains)
            .MaximumLength(100);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200);
    }
}
