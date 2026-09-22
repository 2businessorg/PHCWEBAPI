using MediatR;

namespace Clients.Application.Features.DeleteClient;

/// <summary>
/// Command para eliminar um cliente
/// </summary>
public record DeleteClientCommand(
    decimal No,
    decimal Estab = 0
) : IRequest<bool>;
