using MediatR;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.GetAllRoles;

public sealed class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IEnumerable<string>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<GetAllRolesQueryHandler> _logger;

    public GetAllRolesQueryHandler(
        IRoleRepository roleRepository,
        ILogger<GetAllRolesQueryHandler> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _roleRepository.GetAllRolesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all roles");
            return new List<string>();
        }
    }
}
