namespace VatTaxes.Domain.Entities;

/// <summary>
/// Entidade de taxa de IVA
/// </summary>
public class VatTax
{
    public string VatTaxStamp { get; set; } = null!;
    public decimal Codigo { get; set; }
    public decimal Taxa { get; set; }
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
    public bool Marcada { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string Design { get; set; } = string.Empty;
}
