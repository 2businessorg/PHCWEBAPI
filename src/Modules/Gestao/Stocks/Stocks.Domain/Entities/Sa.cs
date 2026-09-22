namespace Stocks.Domain.Entities;

/// <summary>
/// SA - Stock por Armazém
/// </summary>
public class Sa
{
    public string? Sastamp { get; set; }
    public decimal Stock { get; set; }
    public string? Ref { get; set; }
    public int Armazem { get; set; }
    public string? Local { get; set; }
    public decimal Optimo { get; set; }
    public decimal Rescli { get; set; }
    public decimal Resfor { get; set; }
    public decimal Qttrec { get; set; }
    public decimal Eoq { get; set; }
    public decimal Stmin { get; set; }
    public decimal Consumo { get; set; }
    public decimal Ptoenc { get; set; }
    public decimal Qttacin { get; set; }
    public decimal Pcpond { get; set; }
    public decimal Epcpond { get; set; }
    public decimal Rescat { get; set; }
    public decimal Epv { get; set; }
    public decimal Pv { get; set; }
    public string? Ousrinis { get; set; }
    public DateTime Ousrdata { get; set; }
    public string? Ousrhora { get; set; }
    public string? Usrinis { get; set; }
    public DateTime Usrdata { get; set; }
    public string? Usrhora { get; set; }
    public bool Marcada { get; set; }
}
