using FluentValidation;
using Currencies.Application.Errors;
using Currencies.Domain.Repositories;

namespace Currencies.Application.Features.CreateCurrency;

/// <summary>
/// Validador para CreateCurrencyCommand.
/// </summary>
public sealed class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    private readonly ICurrencyRepository _currencyRepository;

    /// <summary>
    /// Inicializa validador com regras de validação.
    /// </summary>
    public CreateCurrencyCommandValidator(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;

        RuleFor(x => x.Dto.Moeda)
            .NotEmpty().WithMessage("Moeda é obrigatória").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .Length(1, 3).WithMessage("Moeda deve ter entre 1 e 3 caracteres").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .Matches(@"^[A-Z]{2,3}$").WithMessage("Moeda deve conter apenas letras maiúsculas").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .MustAsync((moeda, ct) => BeUniqueMoeda(moeda, ct)).WithMessage("Moeda '{PropertyValue}' já existe").WithErrorCode(CurrenciesErrorCatalog.CurrencyAlreadyExists.Code);

        RuleFor(x => x.Dto.Pais)
            .NotEmpty().WithMessage("País é obrigatório").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code)
            .MaximumLength(12).WithMessage("País não pode ter mais de 12 caracteres").WithErrorCode(CurrenciesErrorCatalog.ValidationError.Code);
    }

    private async Task<bool> BeUniqueMoeda(string moeda, CancellationToken cancellationToken)
    {
        var exists = await _currencyRepository.ExistsAsync(moeda, cancellationToken);
        return !exists;
    }
}
