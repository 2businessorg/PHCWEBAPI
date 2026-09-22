namespace Dossiers.Domain.Entities;

/// <summary>
/// Entidade que representa Contactos (EM)
/// </summary>
public class Em
{
    public decimal No { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Ncont { get; set; } = string.Empty;
    public string Morada { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Codpost { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Segmento { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpostl { get; set; } = string.Empty;
    public string Emstamp { get; set; } = string.Empty;
}
