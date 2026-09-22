using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Features.CreateRole;

public sealed record CreateRoleCommand(
    string RoleName) : IRequest<RoleResponseDto>;
