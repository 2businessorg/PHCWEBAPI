using FluentValidation;
using Currencies.Application.Errors;

namespace Currencies.Application.Features.UpdateCurrency;

/// <summary>
/// Validador para UpdateCurrencyCommand.
/// </summary>
public sealed class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Moeda)
            .NotEmpty().WithMessage("Moeda é obrigatória").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .Length(1, 3).WithMessage("Moeda deve ter entre 1 e 3 caracteres").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .Matches(@"^[A-Z]{2,3}$").WithMessage("Moeda deve conter apenas letras maiúsculas").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code);

        RuleFor(x => x.Pais)
            .NotEmpty().WithMessage("País é obrigatório").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .MaximumLength(12).WithMessage("País não pode ter mais de 12 caracteres").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code);
    }
}
