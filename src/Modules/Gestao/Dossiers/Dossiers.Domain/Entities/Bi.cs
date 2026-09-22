namespace Dossiers.Domain.Entities;

/// <summary>
/// Linha de dossier (BI)
/// </summary>
public class Bi
{
    public string Bistamp { get; set; } = string.Empty;
    public string Bostamp { get; set; } = string.Empty;
    public string Ref { get; set; } = string.Empty;
    public string Design { get; set; } = string.Empty;
    public decimal Qtt { get; set; }
    public decimal Tabiva { get; set; }
    public decimal Iva { get; set; }
    public bool Ivaincl { get; set; }
    public decimal Debito { get; set; }
    public decimal Ttdeb { get; set; }

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
}
