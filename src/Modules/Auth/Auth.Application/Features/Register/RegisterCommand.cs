using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Register;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password) : IRequest<RegisterResponseDto>;
