using FluentValidation;
using Invoices.Application.DTOs;

namespace Invoices.Application.Features.CreateFatura;

/// <summary>
/// Validator para CreateFaturaCommand
/// </summary>
public class CreateFaturaCommandValidator : AbstractValidator<CreateFaturaCommand>
{
    public CreateFaturaCommandValidator(CreateInvoiceInputDTOValidator requestValidator)
    {
        RuleFor(x => x.Request)
            .NotNull()
            .SetValidator(requestValidator);
    }
}

/// <summary>
/// Validator para CreateInvoiceInputDTO
/// Valida campos obrigatórios, formatos e regras de negócio
/// </summary>
public class CreateInvoiceInputDTOValidator : AbstractValidator<CreateInvoiceInputDTO>
{
    public CreateInvoiceInputDTOValidator()
    {
        RuleFor(f => f.Ndoc)
            .GreaterThan(0)
            .WithMessage("Ndoc (tipo de documento) deve ser maior que 0");

        RuleFor(f => f.No)
            .GreaterThan(0)
            .WithMessage("No (número de cliente) deve ser maior que 0");

        RuleFor(f => f.Linhas)
            .NotNull()
            .WithMessage("Linhas é obrigatório")
            .Must(l => l != null && l.Count > 0)
            .WithMessage("Deve haver pelo menos uma linha na fatura");

        RuleFor(f => f.Ftano)
            .GreaterThan(1900)
            .When(f => f.Ftano.HasValue)
            .WithMessage("Ftano (ano) deve ser maior que 1900");

        RuleFor(f => f.Estab)
            .GreaterThanOrEqualTo(0)
            .When(f => f.Estab.HasValue)
            .WithMessage("Estab (estabelecimento) deve ser maior ou igual a 0");

        RuleFor(f => f.Nome)
            .MaximumLength(80)
            .When(f => !string.IsNullOrEmpty(f.Nome))
            .WithMessage("Nome não pode exceder 80 caracteres");

        RuleFor(f => f.Moeda)
            .MaximumLength(3)
            .When(f => !string.IsNullOrEmpty(f.Moeda))
            .WithMessage("Moeda não pode exceder 3 caracteres");

        RuleFor(f => f.Data)
            .Must(BeValidDate)
            .When(f => !string.IsNullOrEmpty(f.Data))
            .WithMessage("Data deve estar em formato válido (YYYY-MM-DD)");

        RuleFor(f => f.Observacoes)
            .MaximumLength(500)
            .When(f => !string.IsNullOrEmpty(f.Observacoes))
            .WithMessage("Observações não pode exceder 500 caracteres");

        RuleForEach(f => f.Linhas)
            .SetValidator(new CreateInvoiceLineInputDTOValidator());
    }

    private bool BeValidDate(string date)
    {
        return System.DateTime.TryParse(date, out _);
    }
}

/// <summary>
/// Validator para CreateInvoiceLineInputDTO
/// </summary>
public class CreateInvoiceLineInputDTOValidator : AbstractValidator<CreateInvoiceLineInputDTO>
{
    public CreateInvoiceLineInputDTOValidator()
    {
        RuleFor(l => l.Ref)
            .NotEmpty()
            .WithMessage("Ref (referência do artigo) é obrigatória");

        RuleFor(l => l.Qtt)
            .GreaterThan(0)
            .WithMessage("Qtt (quantidade) deve ser maior que 0");

        RuleFor(l => l.PV)
            .GreaterThanOrEqualTo(0)
            .When(l => l.PV.HasValue)
            .WithMessage("PV (Preço de venda) deve ser maior ou igual a 0");
    }
}
