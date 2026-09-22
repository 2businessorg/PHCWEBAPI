using Dossiers.Application.DTOs;
using MediatR;

namespace Dossiers.Application.Features.GetDossierTypes;

/// <summary>
/// Query para obter tipos de dossier
/// </summary>
public record GetDossierTypesQuery : IRequest<IReadOnlyList<DossierTypeOutputDTO>>;
