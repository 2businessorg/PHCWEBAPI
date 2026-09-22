using Currencies.Application.Errors;
using FluentValidation;

namespace Currencies.Application.Features.CreateCurrencyExchangeRate;

/// <summary>
/// Validador para CreateCurrencyExchangeRateCommand.
/// </summary>
public sealed class CreateCurrencyExchangeRateCommandValidator : AbstractValidator<CreateCurrencyExchangeRateCommand>
{
    public CreateCurrencyExchangeRateCommandValidator()
    {
        RuleFor(x => x.Dto.Moeda)
            .NotEmpty().WithMessage("Moeda é obrigatória").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .Length(2, 3).WithMessage("Moeda deve ter entre 2 e 3 caracteres").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .Matches("^[A-Z]{2,3}$").WithMessage("Moeda deve conter apenas letras maiúsculas").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code);

        RuleFor(x => x.Dto.Pais)
            .NotEmpty().WithMessage("País é obrigatório").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .MaximumLength(12).WithMessage("País não pode ter mais de 12 caracteres").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code);

        RuleFor(x => x.Dto.UCambioC)
            .GreaterThan(0).WithMessage("BuyRate deve ser maior que zero").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code);

        RuleFor(x => x.Dto.UCambioV)
            .GreaterThan(0).WithMessage("SellRate deve ser maior que zero").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code);
    }
}
