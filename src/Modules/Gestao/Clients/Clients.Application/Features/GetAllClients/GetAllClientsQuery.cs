using MediatR;
using Clients.Application.DTOs;

namespace Clients.Application.Features.GetAllClients;

/// <summary>
/// Query para buscar todos os clientes
/// </summary>
public record GetAllClientsQuery(
	decimal? No = null,
	string? Ncont = null,
	string? Nome = null,
	string? Telefone = null,
	string? Morada = null,
	decimal? Estab = null,
	int Page = 1,
	int PageSize = 20
) : IRequest<GetAllClientsResultDTO>;
