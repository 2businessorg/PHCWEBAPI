using System.Security.Claims;

namespace Shared.Kernel.MultiTenancy;

/// <summary>
/// Tenant context interface - resolves database credentials for the current user
/// Scoped service that provides dynamic connection strings to DbContexts
/// 
/// Placed in Shared.Kernel so all modules can reference it without circular dependencies
/// Implementation: Auth.Infrastructure.Services.TenantContext
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the AppLicense stamp for the current tenant
    /// </summary>
    string? AppLicenseStamp { get; }

    /// <summary>
    /// Gets the database server address
    /// </summary>
    string? DbServer { get; }

    /// <summary>
    /// Gets the database name
    /// </summary>
    string? DbDatabase { get; }

    /// <summary>
    /// Gets the database user ID
    /// </summary>
    string? DbUserId { get; }

    /// <summary>
    /// Gets the database password
    /// </summary>
    string? DbPassword { get; }

    /// <summary>
    /// Gets the complete connection string for the tenant's database
    /// </summary>
    string GetConnectionString();

    /// <summary>
    /// Indicates if the current tenant has valid database credentials
    /// </summary>
    bool HasDatabaseCredentials { get; }

    /// <summary>
    /// Gets the current user's claim principal
    /// </summary>
    ClaimsPrincipal? User { get; }
}
