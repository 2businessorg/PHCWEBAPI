using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Login;

public sealed record LoginCommand(
    string Username,
    string Password) : IRequest<LoginResponseDto>;
