using MediatR;

namespace Dossiers.Application.Features.DeleteDossierById;

/// <summary>
/// Command para eliminar dossier pela chave composta (ndos, obrano, boano)
/// </summary>
public record DeleteDossierByIdCommand(decimal Ndos, decimal Obrano, decimal Boano) : IRequest<bool>;
