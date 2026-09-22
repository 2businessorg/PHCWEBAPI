using FluentValidation;
using Clients.Application.Errors;

namespace Clients.Application.Features.CreateClient;

/// <summary>
/// Validador para CreateClientCommand
/// </summary>
public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Dto.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(55).WithMessage("Nome não pode exceder 55 caracteres");

        RuleFor(x => x.Dto.Ncont)
            .NotEmpty().WithMessage("NUIT (Ncont) é obrigatório")
            .MaximumLength(9).WithMessage("NUIT (Ncont) não pode exceder 9 caracteres");

        RuleFor(x => x.Dto.Telefone)
            .MaximumLength(60).WithMessage("Telefone não pode exceder 60 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Dto.Telefone));

        RuleFor(x => x.Dto.Morada)
            .MaximumLength(55).WithMessage("Morada não pode exceder 55 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Dto.Morada));

        RuleFor(x => x.Dto.No)
            .GreaterThanOrEqualTo(0).WithMessage("No não pode ser negativo")
            .When(x => x.Dto.No.HasValue);

        RuleFor(x => x.Dto.Estab)
            .GreaterThanOrEqualTo(0).WithMessage("Estab não pode ser negativo")
            .When(x => x.Dto.Estab.HasValue);

        RuleFor(x => x.Dto)
            .Custom((dto, context) =>
            {
                var no = dto.No.GetValueOrDefault();
                var estab = dto.Estab.GetValueOrDefault();

                if (no == 0 && estab != 0)
                {
                    context.AddFailure(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.No),
                        "Quando Estab for diferente de 0, o campo No também deve ser informado.")
                    {
                        ErrorCode = ClientsErrorCatalog.InvalidNoEstabCombination.Code
                    });
                }

                if (no != 0 && estab == 0)
                {
                    context.AddFailure(new FluentValidation.Results.ValidationFailure(
                        nameof(dto.Estab),
                        "Quando No for diferente de 0, o campo Estab também deve ser diferente de 0.")
                    {
                        ErrorCode = ClientsErrorCatalog.InvalidNoEstabCombination.Code
                    });
                }
            });
    }
}
