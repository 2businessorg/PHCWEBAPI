namespace Dossiers.Domain.Entities;

/// <summary>
/// Entidade complementar da linha de dossier (BI2)
/// </summary>
public class Bi2
{
    public string Bi2stamp { get; set; } = string.Empty;
    public string Bostamp { get; set; } = string.Empty;

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
}
