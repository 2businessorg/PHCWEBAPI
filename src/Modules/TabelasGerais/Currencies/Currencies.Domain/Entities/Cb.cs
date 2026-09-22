namespace Currencies.Domain.Entities;

/// <summary>
/// Entidade correspondente à tabela cb.
/// </summary>
public class Cb
{
    public string CbStamp { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string Moeda { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public decimal UCambioC { get; set; }
    public decimal UCambioV { get; set; }
    public decimal Cambio { get; set; }
    public string Obs { get; set; } = string.Empty;
    public decimal Cambio2 { get; set; }
    public decimal Ecambio { get; set; }
    public decimal Ecambio2 { get; set; }
    public bool Zonaeuro { get; set; }
    public string Unisg { get; set; } = string.Empty;
    public string Unipl { get; set; } = string.Empty;
    public string Centsg { get; set; } = string.Empty;
    public string Centpl { get; set; } = string.Empty;
    public decimal Taxa { get; set; }
    public decimal Taxa2 { get; set; }
    public decimal Cambioinvertido { get; set; }
    public decimal Cambioinvertido2 { get; set; }
    public decimal Ecambioinvertido { get; set; }
    public decimal Ecambioinvertido2 { get; set; }
    public string OusrInis { get; set; } = string.Empty;
    public DateTime OusrData { get; set; }
    public string OusrHora { get; set; } = string.Empty;
    public string UsrInis { get; set; } = string.Empty;
    public DateTime UsrData { get; set; }
    public string UsrHora { get; set; } = string.Empty;
    public bool Marcada { get; set; }
}
