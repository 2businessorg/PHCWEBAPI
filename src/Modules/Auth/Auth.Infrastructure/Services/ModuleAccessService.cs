using System.Security.Claims;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services;

public class ModuleAccessService : IModuleAccessService
{
    private const string AppLicenseStampClaim = "applicense_stamp";

    private readonly IAppLicenseModuleRepository _appLicenseModuleRepository;
    private readonly ILogger<ModuleAccessService> _logger;

    public ModuleAccessService(
        IAppLicenseModuleRepository appLicenseModuleRepository,
        ILogger<ModuleAccessService> logger)
    {
        _appLicenseModuleRepository = appLicenseModuleRepository;
        _logger = logger;
    }

    public async Task<bool> CanAccessModuleAsync(
        ClaimsPrincipal user,
        string nomepack,
        CancellationToken cancellationToken = default)
    {
        var u_applicensestamp = user.FindFirstValue(AppLicenseStampClaim);

        if (string.IsNullOrWhiteSpace(u_applicensestamp))
        {
            _logger.LogWarning("Authenticated user without applicense_stamp claim");
            return false;
        }

        return await _appLicenseModuleRepository.HasAccessToModuleAsync(
            u_applicensestamp,
            nomepack,
            cancellationToken);
    }
}
