namespace Advances.Domain.Entities;

/// <summary>
/// Cabeçalho do Recibo de Adiantamento (tabela rd)
/// </summary>
public class Rd
{
    public string Rdstamp { get; set; } = string.Empty;
    public string Nmdoc { get; set; } = string.Empty;
    public decimal Rno { get; set; }
    public DateTime Rdata { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Morada { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Codpost { get; set; } = string.Empty;
    public string Ncont { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string Nib { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Etotal { get; set; }
    public decimal Base { get; set; }
    public decimal Ebase { get; set; }
    public decimal Ndoc { get; set; }
    public decimal No { get; set; }
    public decimal Rdano { get; set; }
    public string Olcodigo { get; set; } = string.Empty;
    public string Moeda { get; set; } = string.Empty;
    public decimal Contado { get; set; }
    public string Ollocal { get; set; } = string.Empty;
    public string Cm { get; set; } = string.Empty;
    public string Cmdesc { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    // Campos de auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
}
