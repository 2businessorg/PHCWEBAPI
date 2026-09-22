using MediatR;

namespace Auth.Application.Features.GetUserRoles;

public sealed record GetUserRolesQuery(
    string Username) : IRequest<IEnumerable<string>>;
