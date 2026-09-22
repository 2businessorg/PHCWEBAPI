using MediatR;
using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using Auth.Domain.Constants;
using Shared.Kernel.Authorization;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authenticationService.RegisterAsync(
                request.Username,
                request.Email,
                request.Password,
                cancellationToken);

            return new RegisterResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                UserId = string.Empty,
                Errors = result.Success ? new List<string>() : new List<string> { result.Message }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
            return new RegisterResponseDto
            {
                Success = false,
                Message = AuthMessages.InternalError,
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
