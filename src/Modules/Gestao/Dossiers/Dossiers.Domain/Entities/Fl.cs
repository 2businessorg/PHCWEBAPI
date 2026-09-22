namespace Dossiers.Domain.Entities;

/// <summary>
/// Entidade que representa Fornecedores (FL)
/// </summary>
public class Fl
{
    public decimal No { get; set; }
    public decimal Estab { get; set; }
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
    public string Flstamp { get; set; } = string.Empty;
}
