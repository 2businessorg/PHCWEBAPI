using FluentValidation;
using Treasury.Application.Errors;

namespace Treasury.Application.Features.GetReconciliationMovements;

/// <summary>
/// Validates <see cref="GetReconciliationMovementsQuery"/>.
/// </summary>
public class GetReconciliationMovementsQueryValidator : AbstractValidator<GetReconciliationMovementsQuery>
{
    public GetReconciliationMovementsQueryValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage(TreasuryErrorCatalog.InvalidDateRange.Description);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000);
    }
}
