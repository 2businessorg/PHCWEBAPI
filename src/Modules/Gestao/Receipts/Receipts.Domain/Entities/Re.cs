namespace Receipts.Domain.Entities;

/// <summary>
/// Cabeçalho do Recibo (tabela re)
/// </summary>
public class Re
{
    public string Restamp { get; set; } = string.Empty;
    public string Nmdoc { get; set; } = string.Empty;
    public decimal Rno { get; set; }
    public DateTime Rdata { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Etotal { get; set; }
    public decimal Ndoc { get; set; }
    public decimal No { get; set; }
    public decimal Reano { get; set; }
    public string Olcodigo { get; set; } = string.Empty;
    public decimal Totalmoeda { get; set; }
    public string Moeda { get; set; } = string.Empty;
    public decimal Contado { get; set; }
    public string Ollocal { get; set; } = string.Empty;
    public string Ccstamp { get; set; } = string.Empty;

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
    public bool Marcada { get; set; }
}
