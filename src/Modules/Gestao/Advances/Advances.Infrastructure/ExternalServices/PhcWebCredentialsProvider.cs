using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.ExternalServices;
using Shared.Kernel.MultiTenancy;

namespace Advances.Infrastructure.ExternalServices;

/// <summary>
/// Fornece as credenciais do PHC WEB a partir do contexto do tenant atual (AppLicense)
/// </summary>
public class PhcWebCredentialsProvider : IPhcWebCredentialsProvider
{
    private readonly ITenantContext _tenantContext;
    private readonly IAppLicenseRepository _appLicenseRepository;
    private readonly ILogger<PhcWebCredentialsProvider> _logger;

    public PhcWebCredentialsProvider(
        ITenantContext tenantContext,
        IAppLicenseRepository appLicenseRepository,
        ILogger<PhcWebCredentialsProvider> logger)
    {
        _tenantContext = tenantContext;
        _appLicenseRepository = appLicenseRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PhcWebCredentials> GetCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var appLicenseStamp = _tenantContext.AppLicenseStamp;
        if (string.IsNullOrEmpty(appLicenseStamp))
            throw new InvalidOperationException("No AppLicense stamp available in current tenant context");

        var appLicense = await _appLicenseRepository.GetByStampAsync(appLicenseStamp, cancellationToken)
            ?? throw new InvalidOperationException($"AppLicense not found: {appLicenseStamp}");

        if (string.IsNullOrEmpty(appLicense.IntranetLink))
            throw new InvalidOperationException($"IntranetLink not configured for AppLicense: {appLicenseStamp}");

        var username = _tenantContext.DbUserId;
        if (string.IsNullOrEmpty(username))
            throw new InvalidOperationException("No DbUserId available in current tenant context");

        var password = YOUR_PASSWORD;
        if (string.IsNullOrEmpty(password))
            throw new InvalidOperationException("No DbPassword available in current tenant context");

        _logger.LogDebug("PHC WEB credentials resolved for AppLicense: {Stamp}", appLicenseStamp);

        return new PhcWebCredentials
        {
            PhcWebUrl = appLicense.IntranetLink + "/ws/wscript.asmx",
            Username = username,
            Password = YOUR_PASSWORD
        };
    }
}
