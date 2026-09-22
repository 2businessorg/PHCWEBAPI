namespace Dossiers.Domain.Entities;

/// <summary>
/// Tipo de dossier (TS)
/// </summary>
public class Ts
{
    public decimal Ndos { get; set; }
    public string Nmdos { get; set; } = string.Empty;
    public string Bdempresas { get; set; } = string.Empty;
    public decimal Qpreco { get; set; }
    public decimal Qprecocusto { get; set; }
}
