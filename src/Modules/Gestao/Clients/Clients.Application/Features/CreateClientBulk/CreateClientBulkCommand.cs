using MediatR;
using Clients.Application.DTOs;

namespace Clients.Application.Features.CreateClientBulk;

/// <summary>
/// Command para criar múltiplos clientes em bulk
/// </summary>
public class CreateClientBulkCommand : IRequest<CreateClientBulkResponseDTO>
{
    public CreateClientBulkInputDTO Dto { get; }
    public string? CreatedBy { get; }

    public CreateClientBulkCommand(CreateClientBulkInputDTO dto, string? createdBy = null)
    {
        Dto = dto;
        CreatedBy = createdBy;
    }
}
