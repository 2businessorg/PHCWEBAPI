namespace Dossiers.Domain.Entities;

/// <summary>
/// Entidade complementar de Dossier (BO3)
/// </summary>
public class Bo3
{
    public string Bo3stamp { get; set; } = string.Empty;

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
}
