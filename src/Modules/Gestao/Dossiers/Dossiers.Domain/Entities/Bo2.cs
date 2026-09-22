namespace Dossiers.Domain.Entities;

/// <summary>
/// Entidade complementar de Dossier (BO2)
/// </summary>
public class Bo2
{
    public string Bo2stamp { get; set; } = null!;
    public decimal Totalciva { get; set; }

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
}
