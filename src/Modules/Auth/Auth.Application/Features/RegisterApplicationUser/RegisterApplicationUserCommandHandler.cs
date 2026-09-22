using MediatR;
using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Authorization;

namespace Auth.Application.Features.RegisterApplicationUser;

public sealed class RegisterApplicationUserCommandHandler : IRequestHandler<RegisterApplicationUserCommand, RegisterApplicationUserResponseDto>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAppLicenseRepository _appLicenseRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterApplicationUserCommandHandler> _logger;

    public RegisterApplicationUserCommandHandler(
        IAuthenticationService authenticationService,
        UserManager<IdentityUser> userManager,
        IAppLicenseRepository appLicenseRepository,
        IUserRepository userRepository,
        ILogger<RegisterApplicationUserCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _userManager = userManager;
        _appLicenseRepository = appLicenseRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<RegisterApplicationUserResponseDto> Handle(
        RegisterApplicationUserCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Registration attempt for user: {Username} with AppLicense stamp: {Stamp}",
                request.Username,
                request.AppLicenseStamp);

            var result = await _authenticationService.RegisterApplicationUserAsync(
                request.Username,
                request.Email,
                request.Password,
                request.AppLicenseStamp,
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Failed registration attempt for user: {Username}. Reason: {Reason}",
                    request.Username,
                    result.Message);

                return new RegisterApplicationUserResponseDto
                {
                    Success = false,
                    Message = result.Message,
                    Errors = new[] { result.Message }
                };
            }

            // Get user ID from AspNetUsers
            var user = await _userManager.FindByNameAsync(request.Username);
            var userId = user?.Id ?? string.Empty;

            // Get tenant name from AppLicense
            var appLicense = await _appLicenseRepository.GetByStampAsync(request.AppLicenseStamp, cancellationToken);
            var tenantName = appLicense?.Name ?? string.Empty;

            // Automatically add ApiUser role to the registered user
            _logger.LogInformation("Adding {Role} role to user: {Username}", AppRoles.ApiUser, request.Username);
            var roleAdded = await _userRepository.AddToRoleAsync(request.Username, AppRoles.ApiUser, cancellationToken);

            if (!roleAdded)
            {
                _logger.LogWarning("Failed to add {Role} role to user: {Username}", AppRoles.ApiUser, request.Username);
                // Don't fail the entire registration, just log the warning
            }

            _logger.LogInformation(
                "User {Username} registered successfully with tenant: {TenantName} and role: {Role}",
                request.Username,
                tenantName,
                AppRoles.ApiUser);

            return new RegisterApplicationUserResponseDto
            {
                Success = true,
                Message = "Application user registered successfully",
                UserId = userId,
                Username = request.Username,
                TenantName = tenantName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for user: {Username}", request.Username);
            return new RegisterApplicationUserResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred during registration",
                Errors = new[] { ex.Message }
            };
        }
    }
}
