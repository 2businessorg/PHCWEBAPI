using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AppLicense
/// Provides data access to AppLicense table which contains tenant database credentials
/// </summary>
public class AppLicenseRepository : IAppLicenseRepository
{
    private readonly PrimaryDbContext _context;
    private readonly ILogger<AppLicenseRepository> _logger;

    public AppLicenseRepository(
        PrimaryDbContext context,
        ILogger<AppLicenseRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AppLicense?> GetByStampAsync(string stamp, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying AppLicense by stamp: {Stamp}", stamp);
            
            var license = await _context.AppLicenses
                .AsNoTracking()
                .FirstOrDefaultAsync(al => al.Stamp == stamp, cancellationToken);

            if (license == null)
            {
                _logger.LogWarning("AppLicense not found with stamp: {Stamp}", stamp);
            }

            return license;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying AppLicense by stamp: {Stamp}", stamp);
            throw;
        }
    }

    public async Task<AppLicense?> GetByAspNetUsersIdAsync(string aspNetUsersId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying AppLicense by AspNetUsersId: {AspNetUsersId}", aspNetUsersId);
            
            var license = await _context.AppLicenses
                .AsNoTracking()
                .FirstOrDefaultAsync(al => al.AspNetUsersId == aspNetUsersId, cancellationToken);

            if (license == null)
            {
                _logger.LogWarning("AppLicense not found with AspNetUsersId: {AspNetUsersId}", aspNetUsersId);
            }

            return license;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying AppLicense by AspNetUsersId: {AspNetUsersId}", aspNetUsersId);
            throw;
        }
    }

    public async Task<AppLicense?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying AppLicense by name: {Name}", name);
            
            var license = await _context.AppLicenses
                .AsNoTracking()
                .FirstOrDefaultAsync(al => al.Name == name && !al.Inactive, cancellationToken);

            if (license == null)
            {
                _logger.LogWarning("AppLicense not found with name: {Name}", name);
            }

            return license;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying AppLicense by name: {Name}", name);
            throw;
        }
    }

    public async Task<AppLicense?> GetByCompanyNumberAsync(decimal companyNo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying AppLicense by company number: {CompanyNo}", companyNo);
            
            var license = await _context.AppLicenses
                .AsNoTracking()
                .FirstOrDefaultAsync(al => al.CompanyNo == companyNo && !al.Inactive, cancellationToken);

            if (license == null)
            {
                _logger.LogWarning("AppLicense not found with company number: {CompanyNo}", companyNo);
            }

            return license;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying AppLicense by company number: {CompanyNo}", companyNo);
            throw;
        }
    }

    public async Task<IEnumerable<AppLicense>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying all active AppLicenses");
            
            var licenses = await _context.AppLicenses
                .AsNoTracking()
                .Where(al => !al.Inactive)
                .OrderBy(al => al.Name)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} active AppLicenses", licenses.Count);

            return licenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying active AppLicenses");
            throw;
        }
    }

    public async Task<bool> UpdateAspNetUsersIdAsync(string stamp, string aspNetUsersId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating AspNetUsersId for AppLicense. Stamp: {Stamp}, UserId: {UserId}", stamp, aspNetUsersId);
            
            var license = await _context.AppLicenses
                .FirstOrDefaultAsync(al => al.Stamp == stamp, cancellationToken);

            if (license == null)
            {
                _logger.LogWarning("AppLicense not found for update. Stamp: {Stamp}", stamp);
                return false;
            }

            // Update the AspNetUsersId
            license.AspNetUsersId = aspNetUsersId;
            license.UpdatedDate = DateTime.UtcNow;
            license.UpdatedTime = DateTime.UtcNow.ToString("HH:mm:ss");

            _context.AppLicenses.Update(license);
            
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            
            if (rowsAffected == 0)
            {
                _logger.LogWarning("Failed to update AspNetUsersId. No rows affected. Stamp: {Stamp}", stamp);
                return false;
            }

            _logger.LogInformation("AspNetUsersId updated successfully. Stamp: {Stamp}, UserId: {UserId}", stamp, aspNetUsersId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AspNetUsersId for AppLicense. Stamp: {Stamp}, UserId: {UserId}", stamp, aspNetUsersId);
            throw;
        }
    }
}
