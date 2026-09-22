using Dossiers.Application.DTOs;
using MediatR;

namespace Dossiers.Application.Features.CreateDossiersBulk;

/// <summary>
/// Command para criar dossiers em lote
/// </summary>
public record CreateDossiersBulkCommand(CreateDossiersBulkInputDTO Dto, string? CreatedBy = null) : IRequest<CreateDossiersBulkResponseDTO>;
