using MediatR;
using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using Auth.Domain.Constants;
using Shared.Kernel.Authorization;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<LoginCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);
            
            var result = await _authenticationService.LoginAsync(
                request.Username,
                request.Password,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("User {Username} logged in successfully", request.Username);
            }
            else
            {
                _logger.LogWarning("Failed login attempt for user: {Username}. Reason: {Reason}", 
                    request.Username, result.Message);
            }

            return new LoginResponseDto
            {
                Token = result.Token,
                Expiration = result.Expiration,
                Allowed = result.Success,
                OutputResponse = result.Success ? AuthMessages.Authenticated : AuthMessages.BadCredentials
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Critical error during login for user {Username}. Exception Type: {ExceptionType}, Message: {Message}, InnerException: {InnerException}", 
                request.Username, 
                ex.GetType().FullName,
                ex.Message,
                ex.InnerException?.Message ?? "None");
            
            return new LoginResponseDto
            {
                Token = string.Empty,
                Expiration = null,
                Allowed = false,
                OutputResponse = AuthMessages.InternalError
            };
        }
    }
}
