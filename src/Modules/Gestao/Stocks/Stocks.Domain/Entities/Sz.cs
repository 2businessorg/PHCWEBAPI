using Newtonsoft.Json;

namespace Stocks.Domain.Entities;

/// <summary>
/// SZ - Nomes de Armazéns
/// </summary>
public partial class Sz
{
    public string Szstamp { get; set; } = string.Empty;
    public decimal No { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Nomecl { get; set; } = string.Empty;
    public decimal Nocl { get; set; }
    public decimal Estabcl { get; set; }
    public string Site { get; set; } = string.Empty;
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
    public bool Marcada { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}
