namespace Receipts.Domain.Entities;

/// <summary>
/// Conta Bancária (tabela bl)
/// Representa as contas bancárias/caixas disponíveis
/// </summary>
public class Bl
{
    public decimal Noconta { get; set; }
    public string Banco { get; set; } = string.Empty;
    public string Conta { get; set; } = string.Empty;
    public bool Inactivo { get; set; }
}
