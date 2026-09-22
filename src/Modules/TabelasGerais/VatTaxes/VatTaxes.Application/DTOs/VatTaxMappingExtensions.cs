using VatTaxes.Domain;

namespace VatTaxes.Application.DTOs;

internal static class VatTaxMappingExtensions
{
    public static VatTaxOutputDTO ToOutput(this TaxasIva source)
        => new()
        {
            Tabiva = source.Codigo,
            Taxa = source.Taxa,
            Ref = source.Ref,
            Design = source.Design
        };
}
