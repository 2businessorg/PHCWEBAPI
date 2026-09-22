namespace Advances.Domain.Entities;

/// <summary>
/// Cliente (tabela cl) — usado para validação no módulo Advances
/// </summary>
public class Cl
{
    public string Clstamp { get; set; } = string.Empty;
    public decimal No { get; set; }
    public decimal Estab { get; set; }
    public string Nome { get; set; } = string.Empty;
}
