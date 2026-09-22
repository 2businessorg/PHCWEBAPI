using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

/// <summary>
/// Repository interface for reading user field definitions from u_addfields.
/// </summary>
public interface IUserFieldRepository
{
    /// <summary>
    /// Returns all active user field definitions for the given tenant, module and table.
    /// </summary>
    Task<IReadOnlyList<UserField>> GetActiveFieldsAsync(
        string appLicenseStamp,
        string moduleName,
        string tableName,
        CancellationToken cancellationToken = default);
}
