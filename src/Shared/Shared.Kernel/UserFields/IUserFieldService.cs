namespace Shared.Kernel.UserFields;

/// <summary>
/// Service responsible for retrieving user field definitions per tenant/module/table.
/// Implementations should cache results to avoid repeated queries to the primary DB.
/// Placed in Shared.Kernel so all modules can inject it without circular dependencies.
/// Implementation: Auth.Infrastructure.Services.UserFieldService
/// </summary>
public interface IUserFieldService
{
    /// <summary>
    /// Returns the active user field definitions for the given tenant, module and table.
    /// Results are cached per (appLicenseStamp + moduleName + tableName) combination.
    /// </summary>
    /// <param name="appLicenseStamp">Tenant identifier from u_applicense</param>
    /// <param name="moduleName">Module name, e.g. "Clients"</param>
    /// <param name="tableName">PHC table name, e.g. "cl"</param>
    Task<IReadOnlyList<UserFieldDefinition>> GetFieldsAsync(
        string appLicenseStamp,
        string moduleName,
        string tableName,
        CancellationToken cancellationToken = default);
}
