using MediatR;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.GetUserRoles;

public sealed class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, IEnumerable<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserRolesQueryHandler> _logger;

    public GetUserRolesQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserRolesQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> Handle(
        GetUserRolesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _userRepository.GetUserRolesAsync(request.Username, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {Username}", request.Username);
            return new List<string>();
        }
    }
}
