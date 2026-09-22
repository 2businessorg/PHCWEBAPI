using Clients.Application.DTOs;

namespace Clients.Application.Features.GetAllClients;

public sealed record GetAllClientsResultDTO(
    int TotalItems,
    int CurrentPage,
    int PageSize,
    IReadOnlyList<ClientOutputDTO> Items);
