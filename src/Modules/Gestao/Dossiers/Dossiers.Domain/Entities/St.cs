namespace Dossiers.Domain.Entities;

/// <summary>
/// Artigo/stock
/// </summary>
public class St
{
    public string Ststamp { get; set; } = null!;
    public string Ref { get; set; } = string.Empty;
    public string Design { get; set; } = string.Empty;
    public decimal Epv1 { get; set; }
    public decimal Epv2 { get; set; }
    public decimal Epv3 { get; set; }
    public decimal Epv4 { get; set; }
    public decimal Epv5 { get; set; }
    public bool Inactivo { get; set; }
    public bool Iva1Incl { get; set; }
    public bool Iva2Incl { get; set; }
    public bool Iva3Incl { get; set; }
    public bool Iva4Incl { get; set; }
    public bool Iva5Incl { get; set; }
    public bool Ivaincl { get; set; }
    public bool IvapcIncl { get; set; }
    public decimal Pv1 { get; set; }
    public decimal Pv2 { get; set; }
    public decimal Pv3 { get; set; }
    public decimal Pv4 { get; set; }
    public decimal Pv5 { get; set; }
    public decimal Qttacin { get; set; }
    public decimal Qttacout { get; set; }
    public decimal Qttcat { get; set; }
    public decimal Qttcli { get; set; }
    public decimal Qttesp { get; set; }
    public decimal Qttfor { get; set; }
    public decimal Qttrec { get; set; }
    public bool Qtttouch { get; set; }
    public decimal Qttvend { get; set; }
    public bool Stns { get; set; }
    public decimal Stmax { get; set; }
    public decimal Stmin { get; set; }
    public decimal Stock { get; set; }
    public bool Stocktch { get; set; }
    public decimal Tabiva { get; set; }
    public string Familia { get; set; } = string.Empty;
    public string Faminome { get; set; } = string.Empty;
    
    // Campos de custo
    public decimal Pcusto { get; set; }
    public decimal Epcusto { get; set; }
    public decimal Pcpond { get; set; }
    public decimal Epcpond { get; set; }
    public decimal Pcult { get; set; }
    public decimal Epcult { get; set; }
    public decimal Cpoc { get; set; }
}
