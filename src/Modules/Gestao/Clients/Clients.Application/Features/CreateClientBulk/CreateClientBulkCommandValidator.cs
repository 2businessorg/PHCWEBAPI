using System;
using FluentValidation;
using Clients.Application.Errors;
using Clients.Application.DTOs;

namespace Clients.Application.Features.CreateClientBulk;

/// <summary>
/// Validador para CreateClientBulkCommand
/// </summary>
public class CreateClientBulkCommandValidator : AbstractValidator<CreateClientBulkCommand>
{
    private const int MaxBulkSize = 100;

    public CreateClientBulkCommandValidator()
    {
        RuleFor(x => x.Dto.Items)
            .NotNull().WithMessage("Items é obrigatório")
            .NotEmpty().WithMessage(ClientsErrorCatalog.BulkEmpty.Description);

        RuleFor(x => x.Dto.Items)
            .Must(items => items != null && items.Count <= MaxBulkSize)
            .WithMessage(string.Format(ClientsErrorCatalog.BulkMaxItemsExceeded.Description, MaxBulkSize))
            .WithErrorCode(ClientsErrorCatalog.BulkMaxItemsExceeded.Code);

        // Validar duplicatas de Ncont dentro do lote
        RuleFor(x => x.Dto.Items)
            .Must(items =>
            {
                if (items == null) return true;
                var nconts = items.Where(i => !string.IsNullOrWhiteSpace(i.Ncont))
                    .Select(i => i.Ncont!.Trim().ToLower())
                    .ToList();
                return nconts.Count == nconts.Distinct().Count();
            })
            .WithMessage(ClientsErrorCatalog.BulkDuplicateNcont.Description)
            .WithErrorCode(ClientsErrorCatalog.BulkDuplicateNcont.Code);

        // Validar cada item individualmente
        RuleForEach(x => x.Dto.Items!)
            .SetValidator(new CreateClientInputBulkItemValidator());
    }
}

/// <summary>
/// Validador para cada item individual no bulk
/// </summary>
public class CreateClientInputBulkItemValidator : AbstractValidator<CreateClientInputDTO>
{
    public CreateClientInputBulkItemValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(55).WithMessage("Nome não pode exceder 55 caracteres");

        RuleFor(x => x.Ncont)
            .NotEmpty().WithMessage("NUIT (Ncont) é obrigatório")
            .MaximumLength(9).WithMessage("NUIT (Ncont) não pode exceder 9 caracteres");

        RuleFor(x => x.Telefone)
            .MaximumLength(60).WithMessage("Telefone não pode exceder 60 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Telefone));

        RuleFor(x => x.Morada)
            .MaximumLength(55).WithMessage("Morada não pode exceder 55 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Morada));

        RuleFor(x => x.No)
            .GreaterThanOrEqualTo(0).WithMessage("No não pode ser negativo")
            .When(x => x.No.HasValue);

        RuleFor(x => x.Estab)
            .GreaterThanOrEqualTo(0).WithMessage("Estab não pode ser negativo")
            .When(x => x.Estab.HasValue);

        // Validar regras de No/Estab
        RuleFor(x => x)
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
