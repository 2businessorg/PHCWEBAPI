using FluentValidation;

namespace Providers.Application.Features.CreateProvider;

public class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    private static readonly string[] ValidEnvironments = { "Development", "Staging", "Production" };

    public CreateProviderCommandValidator()
    {
        RuleFor(x => x.Dto.Provedor)
            .NotEmpty().WithMessage("Provedor é obrigatório")
            .MaximumLength(50).WithMessage("Provedor não pode exceder 50 caracteres");

        RuleFor(x => x.Dto.Environment)
            .NotEmpty().WithMessage("Environment é obrigatório")
            .Must(env => ValidEnvironments.Contains(env))
            .WithMessage("Environment deve ser: Development, Staging ou Production");

        RuleFor(x => x.Dto.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(200).WithMessage("Descrição não pode exceder 200 caracteres");

        // Validação das linhas (Values)
        RuleFor(x => x.Dto.Values)
            .NotNull().WithMessage("Values não pode ser null");

        RuleForEach(x => x.Dto.Values).ChildRules(value =>
        {
            value.RuleFor(v => v.OperationCode)
                .NotEmpty().WithMessage("OperationCode é obrigatório")
                .MaximumLength(50).WithMessage("OperationCode não pode exceder 50 caracteres");

            value.RuleFor(v => v.Chave)
                .NotEmpty().WithMessage("Chave é obrigatória")
                .MaximumLength(100).WithMessage("Chave não pode exceder 100 caracteres");

            value.RuleFor(v => v.Valor)
                .NotEmpty().WithMessage("Valor é obrigatório")
                .MaximumLength(500).WithMessage("Valor não pode exceder 500 caracteres");

            value.RuleFor(v => v.Ordem)
                .GreaterThanOrEqualTo(0).WithMessage("Ordem deve ser maior ou igual a 0");
        });
    }
}
