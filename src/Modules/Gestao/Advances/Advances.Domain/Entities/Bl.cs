namespace Advances.Domain.Entities;

/// <summary>
/// Conta bancária/caixa (tabela bl)
/// </summary>
public class Bl
{
    public decimal Noconta { get; set; }
    public string Banco { get; set; } = string.Empty;
    public string Conta { get; set; } = string.Empty;
    public bool Inactivo { get; set; }
}
