using FluentValidation;

namespace Dossiers.Application.Features.CreateDossiersBulk;

public class CreateDossiersBulkCommandValidator : AbstractValidator<CreateDossiersBulkCommand>
{
    public CreateDossiersBulkCommandValidator()
    {
        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("Lote de dossiers não pode estar vazio")
            .Must(items => items.Count <= 100).WithMessage("Lote de dossiers não pode exceder 100 itens");
    }
}
