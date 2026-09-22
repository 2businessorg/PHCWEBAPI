namespace Auth.Domain.Entities;

/// <summary>
/// Linha de licenciamento de uma AppLicense. Mapeia dbo.u_applicl.
///
/// Uma licença (<see cref="AppLicense"/>) só tem acesso geral à API quando existe
/// uma linha aqui com <see cref="Type"/> igual a <see cref="ApiPhcType"/> associada
/// ao seu stamp (u_applicensestamp) e não inactiva. Os módulos concretamente
/// autorizados (ex.: "Gestão") ficam em dbo.u_apilic, ligados a esta linha através
/// de u_appliclstamp - ver <see cref="UApiLic"/>.
/// </summary>
public class AppLicenseLine
{
    /// <summary>
    /// Tipo de linha que concede acesso geral à API PHC.
    /// </summary>
    public const string ApiPhcType = "API PHC";

    /// <summary>
    /// Unique identifier (timestamp-based). Maps to u_appliclstamp.
    /// </summary>
    public string Stamp { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da linha (ex.: "API PHC"). Maps to tipo.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Stamp da AppLicense a que esta linha pertence. Maps to u_applicensestamp.
    /// </summary>
    public string AppLicenseStamp { get; set; } = string.Empty;

    /// <summary>
    /// Subtipo da linha. Maps to subtipo.
    /// </summary>
    public string SubType { get; set; } = string.Empty;

    /// <summary>
    /// Nome/descrição da linha. Maps to nome.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Data de início de vigência. Maps to dinicio.
    /// Não é validada nas regras de acesso actuais (ver ADR de 2026-08).
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Data de fim de vigência. Maps to dfim.
    /// Não é validada nas regras de acesso actuais (ver ADR de 2026-08).
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Quantidade. Maps to qtt.
    /// </summary>
    public decimal Quantity { get; set; }

    // Audit fields for creation
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedTime { get; set; } = string.Empty;

    // Audit fields for updates
    public string UpdatedByUsername { get; set; } = string.Empty;
    public DateTime UpdatedDate { get; set; }
    public string UpdatedTime { get; set; } = string.Empty;

    /// <summary>
    /// Mark for deletion/archiving. Maps to marcada.
    /// </summary>
    public bool Marked { get; set; }

    /// <summary>
    /// Nome da divisão. Maps to nomediv.
    /// </summary>
    public string DivisionName { get; set; } = string.Empty;

    /// <summary>
    /// Establishment number. Maps to estab.
    /// </summary>
    public decimal EstablishmentNo { get; set; }

    /// <summary>
    /// Company number. Maps to no.
    /// </summary>
    public decimal CompanyNo { get; set; }

    /// <summary>
    /// Is this line inactive? Maps to inactivo.
    /// </summary>
    public bool Inactive { get; set; }

    /// <summary>
    /// Indica se esta linha concede acesso geral à API PHC.
    /// Apenas verifica existência/tipo/estado - as datas dinicio/dfim são
    /// propositadamente ignoradas por agora.
    /// </summary>
    public bool GrantsApiAccess()
        => !Inactive && string.Equals(Type.Trim(), ApiPhcType, StringComparison.OrdinalIgnoreCase);
}
