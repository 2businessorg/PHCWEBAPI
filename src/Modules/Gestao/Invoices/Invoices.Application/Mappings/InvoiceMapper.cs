using Invoices.Application.DTOs;
using Invoices.Domain.Entities;

namespace Invoices.Application.Mappings;

public static class InvoiceMapper
{
    public static InvoiceOutputDTO ToOutput(this Ft fatura, IEnumerable<Fi>? linhas = null)
    {
        var lineList = linhas?.ToList() ?? new List<Fi>();

        return new InvoiceOutputDTO
        {
            Ndoc = (int)fatura.Ndoc,
            NmDoc = (fatura.NmDoc ?? string.Empty).Trim(),
            Fno = (int)fatura.Fno,
            Ftano = (int)fatura.FtAno,
            No = (int)fatura.No,
            Nome = (fatura.Nome ?? string.Empty).Trim(),
            Estab = (int)fatura.Estab,
            Data = fatura.FData.ToString("yyyy-MM-dd"),
            Moeda = (fatura.Moeda ?? string.Empty).Trim(),
            Ttiva = fatura.TtIva,
            TMIva = fatura.TMIva,
            Total = fatura.Total,
            TotalMoeda = fatura.TotalMoeda,
            Observacoes = string.Empty,
            Linhas = lineList.Select(l => new InvoiceLineOutputDTO
            {
                Ref = (l.Ref ?? string.Empty).Trim(),
                Design = (l.Design ?? string.Empty).Trim(),
                Qtt = l.Qtt,
                Armazem = (int)l.Armazem,
                Pv = l.Pv,
                Pvmoeda = l.Pvmoeda,
                Tabiva = (int)l.Tabiva,
                Iva = l.Iva,
                IvaIncl = l.Ivaincl,
                Ttdeb = l.Tiliquido,
                Tmoeda = l.Tmoeda,
                Lote = (l.Lote ?? string.Empty).Trim()
            }).ToList()
        };
    }
}
