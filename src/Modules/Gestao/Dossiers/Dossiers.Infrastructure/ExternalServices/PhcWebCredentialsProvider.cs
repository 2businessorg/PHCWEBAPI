using Auth.Domain.Interfaces;
using Shared.Abstractions.ExternalServices;
using Shared.Kernel.MultiTenancy;
using Microsoft.Extensions.Logging;

namespace Dossiers.Infrastructure.ExternalServices;

/// <summary>
/// Implementation of IPhcWebCredentialsProvider
/// Resolves PHC WEB credentials from the current tenant context and AppLicense data
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

    /// <summary>
    /// Gets the PHC WEB credentials from the current tenant's AppLicense
    /// </summary>
    public async Task<PhcWebCredentials> GetCredentialsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var appLicenseStamp = _tenantContext.AppLicenseStamp;
            if (string.IsNullOrEmpty(appLicenseStamp))
            {
                throw new InvalidOperationException("No AppLicense stamp available in current tenant context");
            }

            var appLicense = await _appLicenseRepository.GetByStampAsync(appLicenseStamp, cancellationToken);
            if (appLicense == null)
            {
                throw new InvalidOperationException($"AppLicense not found with stamp: {appLicenseStamp}");
            }

            if (string.IsNullOrEmpty(appLicense.IntranetLink))
            {
                throw new InvalidOperationException($"IntranetLink (linkintranet) not configured for AppLicense: {appLicenseStamp}");
            }

            var username = _tenantContext.DbUserId;
            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("No DbUserId available in current tenant context");
            }

            var password = _tenantContext.DbPassword;
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("No DbPassword available in current tenant context");
            }

            _logger.LogInformation("PHC WEB credentials resolved for tenant {Stamp}", appLicenseStamp);
            
            string phcUrl = appLicense.IntranetLink + "/ws/wscript.asmx"; // Endpoint SOAP do PHC WEB
            
            return new PhcWebCredentials
            {
                PhcWebUrl = phcUrl,
                Username = username,
                Password = password
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving PHC WEB credentials from tenant context");
            throw;
        }
    }
}
