using MediatR;
using Clients.Application.DTOs;

namespace Clients.Application.Features.GetClientByNo;

/// <summary>
/// Query para buscar cliente pelo No + Estab
/// </summary>
public record GetClientByNoQuery(
    decimal No,
    decimal Estab
) : IRequest<ClientOutputDTO?>;
