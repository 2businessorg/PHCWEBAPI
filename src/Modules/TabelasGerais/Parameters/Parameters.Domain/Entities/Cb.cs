namespace Parameters.Domain.Entities;

/// <summary>
/// Entidade correspondente à tabela cb (taxas de câmbio / moedas).
/// </summary>
public class Cb
{
    public string Cbstamp { get; set; } = string.Empty;
    public string Moeda { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal? Taxac { get; set; }
    public decimal? Taxav { get; set; }
}
