using FluentValidation;
using Stocks.Application.Errors;

namespace Stocks.Application.Features.CreateStock;

public sealed class CreateStockCommandValidator : AbstractValidator<CreateStockCommand>
{
    public CreateStockCommandValidator()
    {
        RuleFor(x => x.Dto.Referencia)
            .NotEmpty().WithMessage("A referência é obrigatória.")
            .MaximumLength(18).WithMessage("A referência deve ter no máximo 18 caracteres.")
            .WithErrorCode(StocksErrorCatalog.InvalidReference.Code);

        RuleFor(x => x.Dto.Descricao)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(60).WithMessage("A descrição deve ter no máximo 60 caracteres.");

        // Preços são opcionais, mas se fornecidos devem ser válidos
        RuleForEach(x => x.Dto.Precos)
            .ChildRules(preco =>
            {
                preco.RuleFor(p => p.Tabela)
                    .GreaterThanOrEqualTo(1).WithMessage("A tabela deve estar entre 1 e 5.")
                    .LessThanOrEqualTo(5).WithMessage("A tabela deve estar entre 1 e 5.")
                    .WithErrorCode(StocksErrorCatalog.InvalidPriceTable.Code);

                preco.RuleFor(p => p.Valor)
                    .GreaterThanOrEqualTo(0).WithMessage("O valor do preço deve ser maior ou igual a 0.");
            }).When(x => x.Dto.Precos != null && x.Dto.Precos.Count > 0);

        RuleFor(x => x.Dto.FamiliaRef)
            .MaximumLength(18).WithMessage("A referência de família deve ter no máximo 18 caracteres.");
    }
}
