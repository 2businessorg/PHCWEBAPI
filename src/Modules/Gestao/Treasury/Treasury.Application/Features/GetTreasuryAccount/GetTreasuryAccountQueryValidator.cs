using FluentValidation;

namespace Treasury.Application.Features.GetTreasuryAccount;

/// <summary>
/// Validates <see cref="GetTreasuryAccountQuery"/>.
/// </summary>
public class GetTreasuryAccountQueryValidator : AbstractValidator<GetTreasuryAccountQuery>
{
    public GetTreasuryAccountQueryValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
