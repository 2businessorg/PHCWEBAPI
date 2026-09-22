using MediatR;
using VatTaxes.Application.DTOs;
using VatTaxes.Domain;

namespace VatTaxes.Application.Features.UpdateVatTax;

/// <summary>
/// Handler para atualizar uma taxa de IVA
/// </summary>
public sealed class UpdateVatTaxCommandHandler : IRequestHandler<UpdateVatTaxCommand, VatTaxOutputDTO?>
{
    private readonly IVatTaxRepository _repository;

    public UpdateVatTaxCommandHandler(IVatTaxRepository repository)
    {
        _repository = repository;
    }

    public async Task<VatTaxOutputDTO?> Handle(UpdateVatTaxCommand command, CancellationToken cancellationToken)
    {
        // Verificar se a taxa existe
        var taxasIva = await _repository.GetByCodeAsync(command.Code, cancellationToken);
        if (taxasIva == null)
            return null;

        // Atualizar os campos
        taxasIva.Taxa = command.Rate;
        taxasIva.Ref = command.Reference ?? string.Empty;
        taxasIva.Design = command.Description ?? string.Empty;

        // Atualizar campos de auditoria
        var now = DateTime.Now;
        taxasIva.UsrInis = command.UpdatedBy ?? "PHCAPI";
        taxasIva.UsrData = now.Date;
        taxasIva.UsrHora = now.ToString("HH:mm:ss");

        // Persistir as mudanças
        await _repository.UpdateAsync(taxasIva, cancellationToken);

        // Retornar DTO mapeado
        return taxasIva.ToOutput();
    }
}
