namespace Auth.Domain.Entities;

/// <summary>
/// Mapeamento directo da tabela dbo.u_apilic.
///
/// Cada linha representa um módulo/pacote (<see cref="Nomepack"/>, ex.: "Gestão")
/// autorizado para uma linha de licença. A ligação já não é directa a
/// u_applicense - é feita através de <see cref="AppLicenseLineStamp"/>
/// (u_appliclstamp), que aponta para <see cref="AppLicenseLine"/>.
/// </summary>
public class UApiLic
{
    public string UApilicStamp { get; set; } = string.Empty;

    /// <summary>
    /// Stamp da linha de licença (dbo.u_applicl) a que este módulo pertence.
    /// Maps to u_appliclstamp.
    /// </summary>
    public string AppLicenseLineStamp { get; set; } = string.Empty;

    /// <summary>
    /// Nome do pacote/módulo (ex.: "Gestão"). Maps to nomepack.
    /// </summary>
    public string Nomepack { get; set; } = string.Empty;

    public string Ousrinis { get; set; } = string.Empty;
    public DateTime Ousrdata { get; set; }
    public string Ousrhora { get; set; } = string.Empty;
    public string Usrinis { get; set; } = string.Empty;
    public DateTime Usrdata { get; set; }
    public string Usrhora { get; set; } = string.Empty;

    /// <summary>
    /// Mark for deletion/archiving. Maps to marcada.
    /// </summary>
    public bool Marked { get; set; }

    /// <summary>
    /// Data final. Maps to dfinal. Ignorada nas regras de acesso actuais.
    /// </summary>
    public DateTime FinalDate { get; set; }

    /// <summary>
    /// Data inicial. Maps to dinicial. Ignorada nas regras de acesso actuais.
    /// </summary>
    public DateTime InitialDate { get; set; }
}
