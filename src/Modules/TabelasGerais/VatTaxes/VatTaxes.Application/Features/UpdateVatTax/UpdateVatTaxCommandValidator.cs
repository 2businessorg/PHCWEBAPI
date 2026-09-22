using FluentValidation;

namespace VatTaxes.Application.Features.UpdateVatTax;

/// <summary>
/// Validador para UpdateVatTaxCommand
/// </summary>
public class UpdateVatTaxCommandValidator : AbstractValidator<UpdateVatTaxCommand>
{
    public UpdateVatTaxCommandValidator()
    {
        RuleFor(x => x.Code)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Código de taxa deve ser maior ou igual a 1");

        RuleFor(x => x.Rate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Taxa não pode ser menor que 0");
    }
}
