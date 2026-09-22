using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

/// <summary>
/// Repository for AppLicense data access
/// Used to retrieve database credentials and license information for tenants
/// </summary>
public interface IAppLicenseRepository
{
    /// <summary>
    /// Gets an AppLicense by its unique stamp identifier
    /// </summary>
    Task<AppLicense?> GetByStampAsync(string stamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an AppLicense by AspNetUsers ID (user.Id)
    /// Used during login to find the tenant associated with a user
    /// </summary>
    Task<AppLicense?> GetByAspNetUsersIdAsync(string aspNetUsersId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an AppLicense by company name
    /// </summary>
    Task<AppLicense?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an AppLicense by company number
    /// </summary>
    Task<AppLicense?> GetByCompanyNumberAsync(decimal companyNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active (not inactive) AppLicenses.
    /// NOTA: isto já não implica acesso à API - ver <see cref="IAppLicenseLineRepository"/>.
    /// </summary>
    Task<IEnumerable<AppLicense>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the AspNetUsers ID for an AppLicense
    /// Used during user registration to link license with the created user
    /// </summary>
    Task<bool> UpdateAspNetUsersIdAsync(string stamp, string aspNetUsersId, CancellationToken cancellationToken = default);
}
