namespace Auth.Domain.Entities;

/// <summary>
/// AppLicense entity - stores license and database credentials for multi-tenant scenarios
/// Maps to dbo.u_applicense
///
/// NOTA (2026-08): a validade de acesso à API deixou de ser controlada por campos
/// desta tabela (licensepin, numberofdevices, numberofdays, apicdata, apiedata,
/// apitacesso, expiredate já não existem em u_applicense). Essa validação passou
/// para <see cref="AppLicenseLine"/> (dbo.u_applicl): uma licença só tem acesso geral
/// à API quando existe uma linha com tipo = "API PHC" associada a este stamp.
/// Ver <see cref="Interfaces.IAppLicenseLineRepository"/>.
/// </summary>
public class AppLicense
{
    /// <summary>
    /// Unique identifier (timestamp-based)
    /// Maps to u_applicensestamp
    /// </summary>
    public string Stamp { get; set; } = string.Empty;

    /// <summary>
    /// Is this license inactive?
    /// </summary>
    public bool Inactive { get; set; }

    /// <summary>
    /// License name/company name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Company number
    /// </summary>
    public decimal CompanyNo { get; set; }

    /// <summary>
    /// Establishment number
    /// </summary>
    public decimal EstablishmentNo { get; set; }

    /// <summary>
    /// Contact email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// License creation date
    /// </summary>
    public DateTime CreationDate { get; set; }

    // Audit fields for creation
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedTime { get; set; } = string.Empty;

    // Audit fields for updates
    public string UpdatedByUsername { get; set; } = string.Empty;
    public DateTime UpdatedDate { get; set; }
    public string UpdatedTime { get; set; } = string.Empty;

    /// <summary>
    /// Mark for deletion/archiving
    /// </summary>
    public bool Marked { get; set; }

    // ===== DATABASE CREDENTIALS (TARGET DATABASE) =====
    /// <summary>
    /// SQL Server address/instance for tenant's database
    /// e.g., "192.168.0.25\SQLDEV2022"
    /// </summary>
    public string DbServer { get; set; } = string.Empty;

    /// <summary>
    /// Target database name
    /// e.g., "ONBD_2BMasterPAX"
    /// </summary>
    public string DbDatabase { get; set; } = string.Empty;

    /// <summary>
    /// Database user ID for SQL authentication
    /// </summary>
    public string DbUserId { get; set; } = string.Empty;

    /// <summary>
    /// Database password for SQL authentication
    /// </summary>
    public string DbPassword { get; set; } = string.Empty;

    /// <summary>
    /// Intranet link for this tenant
    /// </summary>
    public string IntranetLink { get; set; } = string.Empty;

    // ===== ASPNET USERS LINK (FOR MULTI-TENANCY) =====
    /// <summary>
    /// Link to the ASP.NET Identity user created for this license
    /// This allows mapping which user belongs to which tenant
    /// Nullable - initially null until user is created and registered
    /// Max length: 50 (standard AspNetUsers.Id length)
    /// </summary>
    public string? AspNetUsersId { get; set; }

    // ===== PHC WEB SERVICE CREDENTIALS =====
    /// <summary>
    /// Dedicated username for the PHC WEB service (wscript.asmx), distinct from the
    /// SQL credentials (csuserid). Maps to "username".
    /// </summary>
    public string WebServiceUsername { get; set; } = string.Empty;

    /// <summary>
    /// Dedicated password for the PHC WEB service, distinct from the SQL credentials
    /// (cspassword). Maps to "password".
    /// </summary>
    public string WebServicePassword { get; set; } = string.Empty;

    /// <summary>
    /// Authentication token for the PHC WEB service. Maps to "tokenauth" (text).
    /// </summary>
    public string WebServiceAuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Whether this license record itself is usable (not inactive/archived).
    /// Does NOT imply API access on its own - see <see cref="AppLicenseLine"/>.
    /// </summary>
    public bool IsActive => !Inactive;

    /// <summary>
    /// Gets the connection string for this tenant's database
    /// </summary>
    public string GetConnectionString()
    {
        // Format: "Server=192.168.0.25\SQLDEV2022;Database=ONBD_2BMasterPAX;User Id=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True;"
        return $"Server={DbServer};Database={DbDatabase};User Id={DbUserId};Password={DbPassword};Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True;";
    }
}
