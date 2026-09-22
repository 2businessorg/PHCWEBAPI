using Dossiers.Application.DTOs;
using MediatR;

namespace Dossiers.Application.Features.CreateDossier;

/// <summary>
/// Command para criação de dossier
/// </summary>
public record CreateDossierCommand(CreateDossierInputDTO Dto, string? CreatedBy = null) : IRequest<DossierOutputDTO>;
