using Auth.Domain.Interfaces;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Repositories;

public class AppLicenseModuleRepository : IAppLicenseModuleRepository
{
    private readonly PrimaryDbContext _context;
    private readonly IAppLicenseLineRepository _appLicenseLineRepository;
    private readonly ILogger<AppLicenseModuleRepository> _logger;

    public AppLicenseModuleRepository(
        PrimaryDbContext context,
        IAppLicenseLineRepository appLicenseLineRepository,
        ILogger<AppLicenseModuleRepository> logger)
    {
        _context = context;
        _appLicenseLineRepository = appLicenseLineRepository;
        _logger = logger;
    }

    public async Task<bool> HasAccessToModuleAsync(
        string u_applicensestamp,
        string nomepack,
        CancellationToken cancellationToken = default)
    {
        var licenseStamp = u_applicensestamp.Trim();
        var moduleName = nomepack.Trim();

        // 1) A licença precisa de ter acesso geral à API (linha "API PHC" em u_applicl)
        var apiLine = await _appLicenseLineRepository.GetApiAccessLineAsync(licenseStamp, cancellationToken);

        if (apiLine == null)
        {
            _logger.LogWarning(
                "Module access denied - no active 'API PHC' line for u_applicensestamp: {Stamp}",
                licenseStamp);
            return false;
        }

        // 2) O módulo pedido precisa de estar autorizado em u_apilic, para essa linha
        var appLicenseLineStamp = apiLine.Stamp.Trim();

        var hasAccess = await _context.u_apilic
            .AsNoTracking()
            .AnyAsync(x =>
                x.AppLicenseLineStamp.Trim() == appLicenseLineStamp &&
                x.Nomepack.Trim() == moduleName,
                cancellationToken);

        _logger.LogDebug(
            "Module access check. u_applicensestamp: {Stamp}, u_appliclstamp: {LineStamp}, nomepack: {Module}, hasAccess: {HasAccess}",
            licenseStamp,
            appLicenseLineStamp,
            moduleName,
            hasAccess);

        return hasAccess;
    }
}
