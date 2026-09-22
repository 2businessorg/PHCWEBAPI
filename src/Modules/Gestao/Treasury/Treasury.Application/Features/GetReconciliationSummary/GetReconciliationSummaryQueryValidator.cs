using FluentValidation;
using Treasury.Application.Errors;

namespace Treasury.Application.Features.GetReconciliationSummary;

/// <summary>
/// Validates <see cref="GetReconciliationSummaryQuery"/>.
/// </summary>
public class GetReconciliationSummaryQueryValidator : AbstractValidator<GetReconciliationSummaryQuery>
{
    public GetReconciliationSummaryQueryValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage(TreasuryErrorCatalog.InvalidDateRange.Description);
    }
}
