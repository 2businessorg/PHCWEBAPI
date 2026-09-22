using MediatR;

namespace Auth.Application.Features.GetAllRoles;

public sealed record GetAllRolesQuery() : IRequest<IEnumerable<string>>;
