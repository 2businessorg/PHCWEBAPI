using Dossiers.Domain.Repositories;
using MediatR;

namespace Dossiers.Application.Features.DeleteDossierById;

/// <summary>
/// Handler para eliminar dossier por chave composta
/// </summary>
public class DeleteDossierByIdCommandHandler : IRequestHandler<DeleteDossierByIdCommand, bool>
{
    private readonly IDossierRepository _repository;

    public DeleteDossierByIdCommandHandler(IDossierRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> Handle(DeleteDossierByIdCommand request, CancellationToken cancellationToken)
        => _repository.DeleteByKeyAsync(request.Ndos, request.Obrano, request.Boano, cancellationToken);
}
