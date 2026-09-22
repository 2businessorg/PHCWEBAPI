using Dossiers.Application.DTOs;
using MediatR;

namespace Dossiers.Application.Features.GetDossierById;

/// <summary>
/// Query para obter dossier pela chave composta (ndos, obrano, boano)
/// </summary>
public record GetDossierByIdQuery(decimal Ndos, decimal Obrano, decimal Boano) : IRequest<DossierOutputDTO?>;
