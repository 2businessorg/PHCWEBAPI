using FluentValidation;

namespace Clients.Application.Features.UpdateClient;

/// <summary>
/// Validador para UpdateClientCommand
/// </summary>
public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.No)
            .GreaterThan(0).WithMessage("No é obrigatório e deve ser maior que zero");

        RuleFor(x => x.Dto.Nome)
            .MaximumLength(55).WithMessage("Nome não pode exceder 55 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Dto.Nome));

        RuleFor(x => x.Dto.Telefone)
            .MaximumLength(60).WithMessage("Telefone não pode exceder 60 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Dto.Telefone));

        RuleFor(x => x.Dto.Morada)
            .MaximumLength(55).WithMessage("Morada não pode exceder 55 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Dto.Morada));

        RuleFor(x => x.Dto.Email)
            .MaximumLength(60).WithMessage("Email não pode exceder 60 caracteres")
            .EmailAddress().WithMessage("Email inválido")
            .When(x => !string.IsNullOrEmpty(x.Dto.Email));
    }
}
