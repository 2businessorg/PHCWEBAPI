using System.Security.Claims;
using Auth.Domain.Interfaces;
using Auth.Domain.Constants;
using Shared.Kernel.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Authentication service implementation using ASP.NET Identity
/// This is the concrete implementation that can be replaced with other providers (Auth0, Firebase, etc.)
/// </summary>
public class IdentityAuthenticationService : IAuthenticationService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IAppLicenseRepository _appLicenseRepository;
    private readonly IAppLicenseLineRepository _appLicenseLineRepository;
    private readonly ICredentialEncryptionService _encryptionService;
    private readonly ILogger<IdentityAuthenticationService> _logger;

    public IdentityAuthenticationService(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService,
        IUserRepository userRepository,
        IAppLicenseRepository appLicenseRepository,
        IAppLicenseLineRepository appLicenseLineRepository,
        ICredentialEncryptionService encryptionService,
        ILogger<IdentityAuthenticationService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _appLicenseRepository = appLicenseRepository;
        _appLicenseLineRepository = appLicenseLineRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting authentication for user: {Username}", username);
            
            var user = await _userManager.FindByNameAsync(username);
            
            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = AuthMessages.BadCredentials
                };
            }

            _logger.LogInformation("User found: {Username}, checking password and lockout status...", username);
            
            // 🛡️ SECURITY: Ensure lockout is enabled for this user (fix for legacy users)
            if (!await _userManager.GetLockoutEnabledAsync(user))
            {
                _logger.LogWarning("Lockout was disabled for user {Username}, enabling it now", username);
                await _userManager.SetLockoutEnabledAsync(user, true);
            }
            
            // 🛡️ SECURITY: Check if user is locked out (VULN-003 Lockout)
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("User {Username} is locked out until {LockoutEnd}", username, user.LockoutEnd);
                
                // ⚠️ SECURITY: Return generic message to prevent user enumeration
                // Attackers should NOT know if account exists or is locked
                // Detailed info only in logs for internal audit
                return new AuthenticationResult
                {
                    Success = false,
                    Message = AuthMessages.BadCredentials  // Generic message - no info leakage
                };
            }
            
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            
            if (!passwordValid)
            {
                // 🛡️ SECURITY: Increment failed login count for lockout
                await _userManager.AccessFailedAsync(user);
                var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                _logger.LogWarning("Invalid password for user: {Username}. Failed attempts: {FailedCount}", username, failedCount);
                
                return new AuthenticationResult
                {
                    Success = false,
                    Message = AuthMessages.BadCredentials
                };
            }
            
            // 🛡️ SECURITY: Reset failed login count on successful login
            await _userManager.ResetAccessFailedCountAsync(user);

            _logger.LogInformation("Password validated for user: {Username}, retrieving roles and AppLicense info...", username);
            
            var userRoles = await _userManager.GetRolesAsync(user);
            
            _logger.LogInformation("User {Username} has {RoleCount} roles", username, userRoles.Count);

            // ===== MULTI-TENANCY: LOAD APPLICENSE CREDENTIALS =====
            _logger.LogInformation("Loading AppLicense information for user: {Username} with UserId: {UserId}", username, user.Id);
            
            // Query by AspNetUsersId to properly map user to license
            var appLicense = await _appLicenseRepository.GetByAspNetUsersIdAsync(user.Id, cancellationToken);

            if (appLicense == null)
            {
                _logger.LogWarning("No AppLicense found with AspNetUsersId: {UserId}", user.Id);
                // Allow login but without database credentials
                // The system may use default database or fail at first API call
            }

            if (appLicense != null)
            {
                var hasApiAccess = !appLicense.Inactive
                    && await _appLicenseLineRepository.HasApiAccessAsync(appLicense.Stamp, cancellationToken);

                if (!hasApiAccess)
                {
                    _logger.LogWarning(
                        "AppLicense {Stamp} is not valid for API access. Inactive: {Inactive}",
                        appLicense.Stamp,
                        appLicense.Inactive);
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "License not valid for API access"
                    };
                }
            }
            
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add AppLicense information to claims (for multi-tenancy resolution)
            if (appLicense != null)
            {
                authClaims.Add(new Claim("applicense_stamp", appLicense.Stamp));
                // Encrypt database credentials before adding to token
                authClaims.Add(new Claim("db_server", _encryptionService.Encrypt(appLicense.DbServer)));
                authClaims.Add(new Claim("db_database", _encryptionService.Encrypt(appLicense.DbDatabase)));
                authClaims.Add(new Claim("db_userid", _encryptionService.Encrypt(appLicense.DbUserId)));
                authClaims.Add(new Claim("db_password", _encryptionService.Encrypt(appLicense.DbPassword)));
                
                _logger.LogInformation("AppLicense {Stamp} loaded for user {Username}", appLicense.Stamp, username);
            }

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            _logger.LogInformation("Generating token for user: {Username}", username);
            
            var token = _tokenService.GenerateToken(authClaims);
            var expiration = _tokenService.GetTokenExpiration();

            _logger.LogInformation("Token generated successfully for user: {Username}", username);

            var result = new AuthenticationResult
            {
                Success = true,
                Token = token,
                Expiration = expiration,
                Message = AuthMessages.Authenticated,
                Roles = userRoles
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Exception in LoginAsync for user {Username}. Type: {ExceptionType}, Message: {Message}", 
                username, 
                ex.GetType().FullName,
                ex.Message);
            throw;
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser != null)
        {
            return new AuthenticationResult
            {
                Success = false,
                Message = AuthMessages.UserAlreadyExists
            };
        }

        var (success, userId, errors) = await _userRepository.CreateUserAsync(username, email, password, cancellationToken);

        return new AuthenticationResult
        {
            Success = success,
            Message = success ? AuthMessages.UserCreated : string.Join(", ", errors)
        };
    }

    /// <summary>
    /// Registers a new application user and links it to an AppLicense
    /// 
    /// FLOW:
    /// 1. Validates that the AppLicense stamp exists and is not yet linked to a user
    /// 2. Creates a new AspNetUser with the provided credentials
    /// 3. Updates the AppLicense with the new user's ID (AspNetUsersId)
    /// 4. Returns success
    /// 
    /// This is used during tenant registration/onboarding
    /// </summary>
    public async Task<AuthenticationResult> RegisterApplicationUserAsync(
        string username, 
        string email, 
        string password, 
        string appLicenseStamp, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting application user registration for username: {Username}, stamp: {Stamp}", username, appLicenseStamp);

            // STEP 1: Validate AppLicense stamp exists
            var appLicense = await _appLicenseRepository.GetByStampAsync(appLicenseStamp, cancellationToken);
            
            if (appLicense == null)
            {
                _logger.LogWarning("AppLicense stamp not found: {Stamp}", appLicenseStamp);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Invalid or non-existent AppLicense stamp"
                };
            }

            // STEP 2: Check if license already has a user
            if (!string.IsNullOrEmpty(appLicense.AspNetUsersId))
            {
                _logger.LogWarning("AppLicense stamp already has a user assigned: {Stamp}, UserId: {UserId}", appLicenseStamp, appLicense.AspNetUsersId);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "This license already has an associated user"
                };
            }

            // STEP 3: Check if license is valid for API access
            var hasApiAccess = !appLicense.Inactive
                && await _appLicenseLineRepository.HasApiAccessAsync(appLicense.Stamp, cancellationToken);

            if (!hasApiAccess)
            {
                _logger.LogWarning("AppLicense is not valid for API access: {Stamp}", appLicenseStamp);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "License is not valid for API access"
                };
            }

            // STEP 4: Check if user already exists
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null)
            {
                _logger.LogWarning("Username already exists: {Username}", username);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = AuthMessages.UserAlreadyExists
                };
            }

            // STEP 5: Create new AspNetUser
            _logger.LogInformation("Creating new user: {Username}, email: {Email}", username, email);
            var (createSuccess, newUserId, createErrors) = await _userRepository.CreateUserAsync(username, email, password, cancellationToken);

            if (!createSuccess || string.IsNullOrEmpty(newUserId))
            {
                _logger.LogError("Failed to create user. Errors: {Errors}", string.Join(", ", createErrors));
                return new AuthenticationResult
                {
                    Success = false,
                    Message = string.Join(", ", createErrors)
                };
            }

            _logger.LogInformation("User created successfully: {Username}, UserId: {UserId}", username, newUserId);

            // STEP 6: Update AppLicense with the new user's ID
            _logger.LogInformation("Updating AppLicense with AspNetUsersId. Stamp: {Stamp}, UserId: {UserId}", appLicenseStamp, newUserId);
            var updateSuccess = await _appLicenseRepository.UpdateAspNetUsersIdAsync(appLicenseStamp, newUserId, cancellationToken);

            if (!updateSuccess)
            {
                _logger.LogError("Failed to update AppLicense AspNetUsersId. Stamp: {Stamp}, UserId: {UserId}", appLicenseStamp, newUserId);
                // At this point, user is created but not linked to license
                // This is a serious issue - log it but return error
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "User created but failed to link to AppLicense"
                };
            }

            _logger.LogInformation("Application user registered successfully. Username: {Username}, Stamp: {Stamp}, License: {License}", 
                username, appLicenseStamp, appLicense.Name);

            return new AuthenticationResult
            {
                Success = true,
                Message = "Application user registered successfully",
                AppLicenseStamp = appLicenseStamp,
                DbServer = appLicense.DbServer,
                DbDatabase = appLicense.DbDatabase,
                DbUserId = appLicense.DbUserId,
                DbPassword = appLicense.DbPassword
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Exception in RegisterApplicationUserAsync. Username: {Username}, Stamp: {Stamp}, Type: {ExceptionType}, Message: {Message}", 
                username, 
                appLicenseStamp,
                ex.GetType().FullName,
                ex.Message);
            throw;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var principal = await _tokenService.ValidateTokenAsync(token);
        return principal != null;
    }

    public Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // Token revocation would require a token blacklist/cache
        // For now, tokens are stateless and expire naturally
        // This can be enhanced with Redis or database-backed token blacklist
        return Task.FromResult(true);
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserRolesAsync(username, cancellationToken);
    }
}
