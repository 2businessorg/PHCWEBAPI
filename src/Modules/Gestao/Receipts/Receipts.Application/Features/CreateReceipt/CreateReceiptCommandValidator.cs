using FluentValidation;

namespace Receipts.Application.Features.CreateReceipt;

/// <summary>
/// Validação de input para criação de Recibo
/// </summary>
public class CreateReceiptCommandValidator : AbstractValidator<CreateReceiptCommand>
{
    public CreateReceiptCommandValidator()
    {
        RuleFor(x => x.Dto.Ndoc)
            .GreaterThan(0)
            .WithMessage("O número da série do recibo (docTypeId) deve ser maior que zero.");

        RuleFor(x => x.Dto.No)
            .GreaterThan(0)
            .WithMessage("O número do cliente (clientId) deve ser maior que zero.");

        RuleFor(x => x.Dto.Contado)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O número da conta bancária (bankAccountId) não pode ser negativo.");

        RuleFor(x => x.Dto.Linhas)
            .NotEmpty()
            .WithMessage("O recibo deve ter pelo menos uma linha de regularização.");

        RuleForEach(x => x.Dto.Linhas).ChildRules(linha =>
        {
            linha.RuleFor(l => l.InvoiceNumber)
                .GreaterThan(0)
                .WithMessage("O número da factura (invoiceNumber) deve ser maior que zero.");

            linha.RuleFor(l => l.InvoiceTypeId)
                .GreaterThan(0)
                .WithMessage("O tipo da factura (invoiceTypeId) deve ser maior que zero.");

            linha.RuleFor(l => l.InvoiceYear)
                .GreaterThan(0)
                .WithMessage("O ano da factura (invoiceYear) deve ser maior que zero.");

            linha.RuleFor(l => l.Amount)
                .GreaterThan(0)
                .WithMessage("O valor a regularizar (amount) deve ser maior que zero.");
        });
    }
}
