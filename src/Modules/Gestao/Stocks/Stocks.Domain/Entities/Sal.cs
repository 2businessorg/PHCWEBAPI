using Newtonsoft.Json;

namespace Stocks.Domain.Entities;

/// <summary>
/// SAL - Stock por Lote/Armazém
/// </summary>
public partial class Sal
{
    public string Salstamp { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public decimal Armazem { get; set; }
    public decimal Qttrec { get; set; }
    public decimal Qttacin { get; set; }
    public decimal Pcpond { get; set; }
    public decimal Epcpond { get; set; }
    public decimal Qttcat { get; set; }
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
    public bool Marcada { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}
