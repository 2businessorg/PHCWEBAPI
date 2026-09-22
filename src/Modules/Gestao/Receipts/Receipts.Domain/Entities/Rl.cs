namespace Receipts.Domain.Entities;

/// <summary>
/// Linha do Recibo (tabela rl)
/// </summary>
public class Rl
{
    public string Rlstamp { get; set; } = string.Empty;
    public decimal Ndoc { get; set; }
    public decimal Rno { get; set; }
    public string Cdesc { get; set; } = string.Empty;
    public decimal Nrdoc { get; set; }
    public decimal Rec { get; set; }
    public decimal Erec { get; set; }
    public decimal Eval { get; set; }
    public DateTime Datalc { get; set; }
    public DateTime Dataven { get; set; }
    public string Restamp { get; set; } = string.Empty;
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
