using Newtonsoft.Json;

namespace Clients.Domain.Entities;

/// <summary>
/// Entidade de Cliente - Tabela CL (Principal)
/// Contém apenas campos utilizados nos DTOs + auditoria
/// </summary>
public partial class Cl
{
    // Identificação
    public string Clstamp { get; set; } = string.Empty;
    public decimal No { get; set; }
    public decimal Estab { get; set; }

    // Dados principais do cliente (usados no DTO)
    public string Nome { get; set; } = string.Empty;
    public string Ncont { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Morada { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Inactivo { get; set; }

    // Auditoria
    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;
    public bool Marcada { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}
