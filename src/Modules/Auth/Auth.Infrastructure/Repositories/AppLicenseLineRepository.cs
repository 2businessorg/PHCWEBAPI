using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Repositories;

public class AppLicenseLineRepository : IAppLicenseLineRepository
{
    private readonly PrimaryDbContext _context;
    private readonly ILogger<AppLicenseLineRepository> _logger;

    public AppLicenseLineRepository(
        PrimaryDbContext context,
        ILogger<AppLicenseLineRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AppLicenseLine?> GetApiAccessLineAsync(string appLicenseStamp, CancellationToken cancellationToken = default)
    {
        var licenseStamp = appLicenseStamp.Trim();

        var line = await _context.AppLicenseLines
            .AsNoTracking()
            .Where(x =>
                x.AppLicenseStamp.Trim() == licenseStamp &&
                !x.Inactive &&
                x.Type.Trim() == AppLicenseLine.ApiPhcType)
            .FirstOrDefaultAsync(cancellationToken);

        _logger.LogDebug(
            "API PHC line lookup. u_applicensestamp: {Stamp}, found: {Found}",
            licenseStamp,
            line != null);

        return line;
    }

    public async Task<bool> HasApiAccessAsync(string appLicenseStamp, CancellationToken cancellationToken = default)
    {
        var line = await GetApiAccessLineAsync(appLicenseStamp, cancellationToken);
        return line != null;
    }
}
