using MediatR;
using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.CreateRole;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleResponseDto>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<CreateRoleCommandHandler> _logger;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        ILogger<CreateRoleCommandHandler> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<RoleResponseDto> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _roleRepository.RoleExistsAsync(request.RoleName, cancellationToken);

            if (exists)
            {
                return new RoleResponseDto
                {
                    Success = false,
                    Message = $"Role {request.RoleName} already exists"
                };
            }

            var success = await _roleRepository.CreateRoleAsync(request.RoleName, cancellationToken);

            return new RoleResponseDto
            {
                Success = success,
                Message = success ? $"Role {request.RoleName} created successfully" : "Failed to create role"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role {RoleName}", request.RoleName);
            return new RoleResponseDto
            {
                Success = false,
                Message = "INTERNAL_ERROR"
            };
        }
    }
}
