using Dossiers.Application.Errors;
using FluentValidation;

namespace Dossiers.Application.Features.CreateDossier;

public class CreateDossierCommandValidator : AbstractValidator<CreateDossierCommand>
{
    public CreateDossierCommandValidator()
    {
        RuleFor(x => x.Dto.Ndos)
            .InclusiveBetween(0, 999)
            .WithErrorCode(DossiersErrorCatalog.InvalidTipoDossier.Code)
            .WithMessage("Número do Tipo de Dossier deve estar entre 0 e 999");

        RuleFor(x => x.Dto.Nome)
            .MaximumLength(55).WithMessage("Nome deve ter no máximo 55 caracteres");

        RuleFor(x => x.Dto.Boano)
            .GreaterThan(1999).When(x => x.Dto.Boano.HasValue).WithMessage("Boano deve ser maior que 1999");

        RuleFor(x => x.Dto.Data)
            .Must(d => !d.HasValue || d.Value.Year > 1999).WithMessage("Ano da data deve ser superior a 1999");

        RuleFor(x => x.Dto.Linhas)
            .NotNull().WithMessage("Lista de linhas é obrigatória");

        RuleForEach(x => x.Dto.Linhas)
            .ChildRules(linha =>
            {
                linha.RuleFor(l => l.Quantidade)
                    .GreaterThan(0)
                    .WithErrorCode(DossiersErrorCatalog.InvalidQuantity.Code)
                    .WithMessage(DossiersErrorCatalog.InvalidQuantity.Description);
            });
    }
}
