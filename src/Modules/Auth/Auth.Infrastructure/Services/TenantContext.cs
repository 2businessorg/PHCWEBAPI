using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Kernel.MultiTenancy;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Implementation of ITenantContext
/// Reads encrypted database credentials from JWT claims
/// Optimized with single-pass initialization and caching
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICredentialEncryptionService _encryptionService;

    private string? _appLicenseStamp;
    private string? _dbServer;
    private string? _dbDatabase;
    private string? _dbUserId;
    private string? _dbPassword;
    private bool _initialized;
    private bool _hasCredentials;

    public TenantContext(
        IHttpContextAccessor httpContextAccessor,
        ICredentialEncryptionService encryptionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _encryptionService = encryptionService;
        _initialized = false;
        _hasCredentials = false;
    }

    public string? AppLicenseStamp
    {
        get
        {
            EnsureInitialized();
            return _appLicenseStamp;
        }
    }

    public string? DbServer
    {
        get
        {
            EnsureInitialized();
            return _dbServer;
        }
    }

    public string? DbDatabase
    {
        get
        {
            EnsureInitialized();
            return _dbDatabase;
        }
    }

    public string? DbUserId
    {
        get
        {
            EnsureInitialized();
            return _dbUserId;
        }
    }

    public string? DbPassword
    {
        get
        {
            EnsureInitialized();
            return _dbPassword;
        }
    }

    public bool HasDatabaseCredentials
    {
        get
        {
            EnsureInitialized();
            return _hasCredentials;
        }
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string GetConnectionString()
    {
        EnsureInitialized();
        
        if (!_hasCredentials)
        {
            throw new InvalidOperationException("No database credentials available for current tenant");
        }

        return $"Server={_dbServer};Database={_dbDatabase};User Id={_dbUserId};Password={_dbPassword};Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True;";
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        var user = User;
        if (user == null)
        {
            _initialized = true;
            return;
        }

        // Single-pass claim extraction and decryption
        _appLicenseStamp = user.FindFirst("applicense_stamp")?.Value;
        _dbServer = DecryptClaim(user, "db_server");
        _dbDatabase = DecryptClaim(user, "db_database");
        _dbUserId = DecryptClaim(user, "db_userid");
        _dbPassword = DecryptClaim(user, "db_password");

        // Cache credentials check result
        _hasCredentials = !string.IsNullOrEmpty(_dbServer) 
            && !string.IsNullOrEmpty(_dbDatabase) 
            && !string.IsNullOrEmpty(_dbUserId) 
            && !string.IsNullOrEmpty(_dbPassword);

        _initialized = true;
    }

    private string? DecryptClaim(ClaimsPrincipal user, string claimType)
    {
        try
        {
            var claim = user.FindFirst(claimType);
            return claim != null ? _encryptionService.Decrypt(claim.Value) : null;
        }
        catch
        {
            return null;
        }
    }
}

