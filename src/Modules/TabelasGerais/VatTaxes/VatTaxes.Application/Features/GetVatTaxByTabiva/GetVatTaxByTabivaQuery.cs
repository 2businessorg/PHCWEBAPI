using MediatR;
using VatTaxes.Application.DTOs;

namespace VatTaxes.Application.Features.GetVatTaxByTabiva;

/// <summary>
/// Query para obter uma taxa de IVA por código (Tabiva)
/// </summary>
public sealed record GetVatTaxByTabivaQuery(int Code) : IRequest<VatTaxOutputDTO?>;
