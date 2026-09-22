using Newtonsoft.Json;

namespace Stocks.Domain.Entities;

/// <summary>
/// SE - Lotes de stock
/// </summary>
public partial class Se
{
    public string Sestamp { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public string Ref { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public DateTime? Validade { get; set; }
    public string Design { get; set; } = string.Empty;
    public decimal Pcult { get; set; }
    public decimal Pcpond { get; set; }
    public decimal Epcult { get; set; }
    public decimal Epcpond { get; set; }
    public DateTime? Usaid { get; set; }
    public DateTime? Uintr { get; set; }
    public decimal Pcimp { get; set; }
    public string Pcmoe { get; set; } = string.Empty;
    public decimal Pcusto { get; set; }
    public DateTime? Data { get; set; }
    public decimal Qttacout { get; set; }
    public decimal Qttacin { get; set; }
    public DateTime? Datafact { get; set; }
    public string Forref { get; set; } = string.Empty;
    public string Forlote { get; set; } = string.Empty;
    public decimal Epcust { get; set; }
    public decimal Epcimp { get; set; }
    public decimal Qttrec { get; set; }
    public string Tipoiect { get; set; } = string.Empty;
    public bool Inactivo { get; set; }
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime? Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime? Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
    public bool Marcada { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}
