using Advances.Application.DTOs;
using MediatR;

namespace Advances.Application.Features.CreateAdvance;

/// <summary>
/// Comando para criação de um Adiantamento via PHC WEB (script insertRdAPI)
/// </summary>
public sealed record CreateAdvanceCommand(
    CreateAdvanceInputDTO Dto,
    string? CreatedBy) : IRequest<CreateAdvanceOutputDTO>;
