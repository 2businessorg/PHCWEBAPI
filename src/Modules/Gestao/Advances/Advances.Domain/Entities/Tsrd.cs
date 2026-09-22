namespace Advances.Domain.Entities;

/// <summary>
/// Série/Tipo de Adiantamento (tabela tsrd)
/// </summary>
public class Tsrd
{
    public string Tsrdstamp { get; set; } = string.Empty;
    public decimal Ndoc { get; set; }
    public string Nmdoc { get; set; } = string.Empty;
    public string Cmcc { get; set; } = string.Empty;
    public string Cmccn { get; set; } = string.Empty;
}
