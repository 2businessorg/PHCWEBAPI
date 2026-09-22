using MediatR;
using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.AddUserToRole;

public sealed class AddUserToRoleCommandHandler : IRequestHandler<AddUserToRoleCommand, RoleResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<AddUserToRoleCommandHandler> _logger;

    public AddUserToRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ILogger<AddUserToRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<RoleResponseDto> Handle(
        AddUserToRoleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var roleExists = await _roleRepository.RoleExistsAsync(request.Role, cancellationToken);

            if (!roleExists)
            {
                return new RoleResponseDto
                {
                    Success = false,
                    Message = $"Role {request.Role} does not exist"
                };
            }

            var success = await _userRepository.AddToRoleAsync(request.Username, request.Role, cancellationToken);

            return new RoleResponseDto
            {
                Success = success,
                Message = success 
                    ? $"User {request.Username} added to role {request.Role}" 
                    : "Failed to add user to role"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {Username} to role {Role}", request.Username, request.Role);
            return new RoleResponseDto
            {
                Success = false,
                Message = "INTERNAL_ERROR"
            };
        }
    }
}
