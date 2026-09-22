namespace Receipts.Domain.Entities;

/// <summary>
/// Configuração da Série de Recibo (tabela tsre)
/// </summary>
public class Tsre
{
    public string Tsrestamp { get; set; } = string.Empty;
    public string Nmdoc { get; set; } = string.Empty;
    public decimal Ndoc { get; set; }
    public decimal Ndino { get; set; }
    public string Ndidesc { get; set; } = string.Empty;

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
    public bool Marcada { get; set; }
}
