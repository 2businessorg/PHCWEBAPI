using Newtonsoft.Json;

namespace Stocks.Domain.Entities;

/// <summary>
/// ST - Lotes de stock
/// </summary>
public partial class St
{
    public string Ststamp { get; set; } = string.Empty;
    public string Ref { get; set; } = string.Empty;
    public string Design { get; set; } = string.Empty;
    public bool Stns { get; set; }
    public string Familia { get; set; } = string.Empty;
    public string Faminome { get; set; } = string.Empty;
    public decimal Epv1 { get; set; }
    public decimal Pv1 { get; set; }
    public bool Iva1incl { get; set; }
    public decimal Epv2 { get; set; }
    public decimal Pv2 { get; set; }
    public bool Iva2incl { get; set; }
    public decimal Epv3 { get; set; }
    public decimal Pv3 { get; set; }
    public bool Iva3incl { get; set; }
    public decimal Epv4 { get; set; }
    public decimal Pv4 { get; set; }
    public bool Iva4incl { get; set; }
    public decimal Epv5 { get; set; }
    public decimal Pv5 { get; set; }
    public bool Iva5incl { get; set; }
    public decimal Epcusto { get; set; }
    public decimal Pcusto { get; set; }
    public decimal Stock { get; set; }
    public decimal Tabiva { get; set; }
    public string Obs { get; set; } = string.Empty;
    public bool Inactivo { get; set; }
    public bool Usalote { get; set; }
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;

    public override string ToString() => JsonConvert.SerializeObject(this);
}
