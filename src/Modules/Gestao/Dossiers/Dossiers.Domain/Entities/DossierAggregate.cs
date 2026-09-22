namespace Dossiers.Domain.Entities;

/// <summary>
/// Agregado de dossier com cabeçalho e linhas
/// </summary>
public class DossierAggregate
{
    public Bo Bo { get; set; } = null!;
    public Bo2 Bo2 { get; set; } = null!;
    public Bo3 Bo3 { get; set; } = null!;
    public List<Bi> Lines { get; set; } = new();
    public List<Bi2> Lines2 { get; set; } = new();
    public List<Bot> TaxTotals { get; set; } = new();
}
