using MediatR;
using Clients.Application.DTOs;

namespace Clients.Application.Features.UpdateClient;

/// <summary>
/// Command para atualizar um cliente existente
/// </summary>
public record UpdateClientCommand(
    decimal No,
    UpdateClientInputDTO Dto,
    string? UpdatedBy,
    decimal Estab = 0
) : IRequest<ClientOutputDTO?>;
