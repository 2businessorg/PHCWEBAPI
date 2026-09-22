using Advances.Application.DTOs;
using FluentValidation;

namespace Advances.Application.Features.CreateAdvance;

/// <summary>
/// Validador para o comando de criação de Adiantamento
/// </summary>
public class CreateAdvanceCommandValidator : AbstractValidator<CreateAdvanceCommand>
{
    public CreateAdvanceCommandValidator()
    {
        RuleFor(x => x.Dto.Ndoc)
            .GreaterThan(0).WithMessage("A série do adiantamento (ndoc) deve ser maior que zero.");

        RuleFor(x => x.Dto.No)
            .GreaterThan(0).WithMessage("O número do cliente (no) deve ser maior que zero.");

        RuleFor(x => x.Dto.Contado)
            .GreaterThanOrEqualTo(0).WithMessage("A conta bancária (contado) deve ser maior ou igual a zero.");

        RuleFor(x => x.Dto.Amount)
            .GreaterThan(0).WithMessage("O valor do adiantamento deve ser maior que zero.");
    }
}
