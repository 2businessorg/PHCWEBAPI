namespace Auth.Domain.Interfaces;

/// <summary>
/// High-level authentication service interface.
/// This abstraction allows for different authentication providers (Identity, Auth0, Firebase, etc.)
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user and returns authentication result
    /// </summary>
    Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<AuthenticationResult> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new application user with ApplicenseLicense stamp
    /// This method validates the stamp, creates the user, and links it to the license
    /// </summary>
    Task<AuthenticationResult> RegisterApplicationUserAsync(string username, string email, string password, string appLicenseStamp, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates if a token is valid
    /// </summary>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes/invalidates a token
    /// </summary>
    Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user roles
    /// </summary>
    Task<IEnumerable<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken = default);
}

/// <summary>
/// Authentication result contract
/// </summary>
public class AuthenticationResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime? Expiration { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = new List<string>();

    // ===== MULTI-TENANCY: DATABASE CREDENTIALS =====
    /// <summary>
    /// AppLicense stamp (unique identifier)
    /// Used to load database credentials for tenant
    /// </summary>
    public string? AppLicenseStamp { get; set; }

    /// <summary>
    /// SQL Server address for tenant's database
    /// e.g., "192.168.0.25\SQLDEV2022"
    /// </summary>
    public string? DbServer { get; set; }

    /// <summary>
    /// Target database name
    /// e.g., "ONBD_2BMasterPAX"
    /// </summary>
    public string? DbDatabase { get; set; }

    /// <summary>
    /// Database user ID for SQL authentication
    /// </summary>
    public string? DbUserId { get; set; }

    /// <summary>
    /// Database password for SQL authentication
    /// </summary>
    public string? DbPassword { get; set; }

    /// <summary>
    /// Gets the connection string for the tenant's database
    /// </summary>
    public string GetConnectionString()
    {
        if (string.IsNullOrEmpty(DbServer) || string.IsNullOrEmpty(DbDatabase) 
            || string.IsNullOrEmpty(DbUserId) || string.IsNullOrEmpty(DbPassword))
        {
            throw new InvalidOperationException("Database credentials are not set in authentication result");
        }

        return $"Server={DbServer};Database={DbDatabase};User Id={DbUserId};Password={DbPassword};Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True;";
    }
}
