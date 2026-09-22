namespace Receipts.Domain.Entities;

/// <summary>
/// Cliente (tabela cl) — usado para validação do no no cabeçalho do recibo
/// </summary>
public class Cl
{
    public string Clstamp { get; set; } = string.Empty;
    public decimal No { get; set; }
    public decimal Estab { get; set; }
    public string Nome { get; set; } = string.Empty;
}
