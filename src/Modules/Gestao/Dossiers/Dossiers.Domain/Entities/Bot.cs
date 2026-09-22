namespace Dossiers.Domain.Entities;

/// <summary>
/// Totais fiscais por código de IVA do dossier (BOT)
/// </summary>
public class Bot
{
    public string Botstamp { get; set; } = string.Empty;
    public string Bostamp { get; set; } = string.Empty;
    public decimal Codigo { get; set; }
    public decimal Taxa { get; set; }
    public decimal Baseinc { get; set; }
    public decimal Ebaseinc { get; set; }
    public decimal Valor { get; set; }
    public decimal Evalor { get; set; }

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
}
