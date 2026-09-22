using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Features.AddUserToRole;

public sealed record AddUserToRoleCommand(
    string Username,
    string Role) : IRequest<RoleResponseDto>;
