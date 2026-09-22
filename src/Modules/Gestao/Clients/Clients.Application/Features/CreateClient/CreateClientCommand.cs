using MediatR;
using Clients.Application.DTOs;

namespace Clients.Application.Features.CreateClient;

/// <summary>
/// Command para criar um novo cliente
/// </summary>
public record CreateClientCommand(
    CreateClientInputDTO Dto,
    string? CreatedBy
) : IRequest<ClientOutputDTO>;
