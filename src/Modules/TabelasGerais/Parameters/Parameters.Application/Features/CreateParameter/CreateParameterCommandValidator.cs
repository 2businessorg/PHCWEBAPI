using FluentValidation;

namespace Parameters.Application.Features.CreateParameter;

public class CreateParameterCommandValidator : AbstractValidator<CreateParameterCommand>
{
    public CreateParameterCommandValidator()
    {
        RuleFor(x => x.Dto.Descricao)
            .NotEmpty().WithMessage("A Descrição é obrigatória")
            .MaximumLength(100).WithMessage("A Descrição não pode exceder 100 caracteres");

        RuleFor(x => x.Dto.Valor)
            .NotEmpty().WithMessage("O Valor é obrigatório")
            .MaximumLength(500).WithMessage("O Valor não pode exceder 500 caracteres");

        RuleFor(x => x.Dto.Tipo)
            .NotEmpty().WithMessage("O Tipo é obrigatório")
            .MaximumLength(50).WithMessage("O Tipo não pode exceder 50 caracteres");

        RuleFor(x => x.Dto.Dec)
            .GreaterThanOrEqualTo(0).WithMessage("Dec deve ser maior ou igual a 0")
            .LessThanOrEqualTo(10).WithMessage("Dec deve ser menor ou igual a 10")
            .When(x => x.Dto.Dec.HasValue);

        // Validação condicional: Dec é obrigatório quando Tipo = "N" (Numérico)
        RuleFor(x => x.Dto.Dec)
            .NotNull().WithMessage("Dec é obrigatório para parâmetros do tipo Numérico")
            .When(x => x.Dto.Tipo == "N");

        RuleFor(x => x.Dto.Tam)
            .GreaterThanOrEqualTo(0).WithMessage("Tam deve ser maior ou igual a 0")
            .When(x => x.Dto.Tam.HasValue);
    }
}
