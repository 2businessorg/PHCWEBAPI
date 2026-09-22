namespace Receipts.Domain.Entities;

/// <summary>
/// Conta Corrente de Cliente (tabela cc) — representa uma factura/documento a regularizar
/// </summary>
public class Cc
{
    public string Ccstamp { get; set; } = string.Empty;
    public decimal No { get; set; }
    public decimal Nrdoc { get; set; }
    public decimal Deb { get; set; }
    public decimal Edeb { get; set; }
    public DateTime Datalc { get; set; }
}
