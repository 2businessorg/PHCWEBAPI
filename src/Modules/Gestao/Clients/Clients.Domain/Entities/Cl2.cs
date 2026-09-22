using Newtonsoft.Json;

namespace Clients.Domain.Entities;

/// <summary>
/// Entidade de Cliente Complementar - Tabela CL2
/// Relacionada 1:1 com CL via Clstamp = Cl2stamp
/// Contém apenas campos de auditoria (nenhum campo é usado diretamente no DTO)
/// </summary>
public partial class Cl2
{
    // Stamp para relacionamento com CL
    public string Cl2stamp { get; set; } = string.Empty;

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
