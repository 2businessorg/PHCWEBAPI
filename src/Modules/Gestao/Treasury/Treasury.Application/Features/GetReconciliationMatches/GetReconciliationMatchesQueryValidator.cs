using FluentValidation;
using Treasury.Application.Errors;

namespace Treasury.Application.Features.GetReconciliationMatches;

/// <summary>
/// Validates <see cref="GetReconciliationMatchesQuery"/>.
/// </summary>
public class GetReconciliationMatchesQueryValidator : AbstractValidator<GetReconciliationMatchesQuery>
{
    public GetReconciliationMatchesQueryValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage(TreasuryErrorCatalog.InvalidDateRange.Description);
    }
}
