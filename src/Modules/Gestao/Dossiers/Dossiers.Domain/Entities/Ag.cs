namespace Dossiers.Domain.Entities;

/// <summary>
/// Entidade que representa Entidades (AG)
/// </summary>
public class Ag
{
    public decimal No { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Ncont { get; set; } = string.Empty;
    public string Morada { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Codpost { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Agstamp { get; set; } = string.Empty;
}
