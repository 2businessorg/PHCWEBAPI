namespace Receipts.Domain.Entities;

/// <summary>
/// Cabeçalho de factura (tabela ft) para joins do módulo Receipts.
/// </summary>
public class Ft
{
    public string Ftstamp { get; set; } = string.Empty;
    public decimal Fno { get; set; }
    public decimal Ftano { get; set; }
    public decimal Ndoc { get; set; }
    public string Nmdoc { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
}
