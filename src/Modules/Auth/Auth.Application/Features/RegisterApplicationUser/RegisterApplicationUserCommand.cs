using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Features.RegisterApplicationUser;

public sealed record RegisterApplicationUserCommand(
    string Username,
    string Email,
    string Password,
    string AppLicenseStamp) : IRequest<RegisterApplicationUserResponseDto>;
