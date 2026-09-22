namespace Dossiers.Domain.Entities;

/// <summary>
/// Cliente mínimo para criação de dossier
/// </summary>
public class DossierClient
{
    public string Clstamp { get; set; } = null!;
    public decimal No { get; set; }
    public decimal Estab { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Ncont { get; set; } = string.Empty;
    public string Morada { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Codpost { get; set; } = string.Empty;
    public string Segmento { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
