using Dossiers.Application.DTOs;
using Dossiers.Domain.Entities;
using System.Globalization;

namespace Dossiers.Application.Mappings;

internal static class DossierMapper
{
    public static DossierOutputDTO ToOutput(this DossierAggregate source, bool includeLines = true)
    {
        return new DossierOutputDTO
        {
            Ndos = source.Bo.Ndos,
            Nmdos = (source.Bo.Nmdos ?? string.Empty).Trim(),
            Obrano = source.Bo.Obrano,
            Boano = source.Bo.Boano,
            No = source.Bo.No,
            Estab = source.Bo.Estab,
            Nome = (source.Bo.Nome ?? string.Empty).Trim(),
            Data = source.Bo.Dataobra,
            Moeda = (source.Bo.Moeda ?? string.Empty).Trim(),
            Total = ToFixed2(source.Bo.UBotot),
            Linhas = includeLines
                ? source.Lines.Select(l =>
                {
                    // Procurar o taxa correspondente ao código de IVA
                    var taxRate = source.TaxTotals
                        .FirstOrDefault(t => t.Codigo == (int)l.Tabiva)
                        ?.Taxa ?? 0m;

                    return new DossierLineOutputDTO
                    {
                        Ref = (l.Ref ?? string.Empty).Trim(),
                        Design = (l.Design ?? string.Empty).Trim(),
                        Qtt = ToFixed2(l.Qtt),
                        tabIva = ToFixed2(l.Iva),
                        IvaIncl = l.Ivaincl,
                        PrecoUnitario = ToFixed2(l.Debito),
                        Total = ToFixed2(l.Ttdeb),
                        iva = ToFixed2(taxRate)
                    };
                }).ToList()
                : new List<DossierLineOutputDTO>()
        };
    }

    private static decimal ToFixed2(decimal value)
        => decimal.Parse(value.ToString("0.00", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
}
