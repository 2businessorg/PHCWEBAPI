using MediatR;
using Clients.Domain.Repositories;

namespace Clients.Application.Features.DeleteClient;

/// <summary>
/// Handler para eliminar um cliente
/// </summary>
public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, bool>
{
    private readonly IClientRepository _repository;

    public DeleteClientCommandHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        DeleteClientCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Buscar cliente
        var (cl, cl2) = await _repository.GetByNoAndEstabAsync(request.No, request.Estab, cancellationToken);

        if (cl == null || cl2 == null)
        {
            return false;
        }

        // 2. Eliminar
        var result = await _repository.DeleteByStampAsync(cl.Clstamp, cancellationToken);

        return result;
    }
}
