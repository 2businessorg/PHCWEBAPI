namespace Dossiers.Domain.Entities;

/// <summary>
/// Entidade principal de Dossier (BO)
/// </summary>
public class Bo
{
    public string Bostamp { get; set; } = string.Empty;
    public decimal Ndos { get; set; } 
    public decimal Obrano { get; set; }
    public decimal Boano { get; set; }
    public decimal No { get; set; }
    public decimal Estab { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Nmdos { get; set; } = string.Empty;
    public DateTime Dataobra { get; set; }
    public string Moeda { get; set; } = string.Empty;
    public decimal UBotot { get; set; }

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
}
