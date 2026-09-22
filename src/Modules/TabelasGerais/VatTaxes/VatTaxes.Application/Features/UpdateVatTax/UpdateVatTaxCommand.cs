using MediatR;
using VatTaxes.Application.DTOs;

namespace VatTaxes.Application.Features.UpdateVatTax;

/// <summary>
/// Comando para atualizar uma taxa de IVA
/// </summary>
public sealed record UpdateVatTaxCommand(
    int Code,
    decimal Rate,
    string? Reference,
    string? Description,
    string? UpdatedBy = null) : IRequest<VatTaxOutputDTO?>;
