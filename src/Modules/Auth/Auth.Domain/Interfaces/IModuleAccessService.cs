using System.Security.Claims;

namespace Auth.Domain.Interfaces;

public interface IModuleAccessService
{
    Task<bool> CanAccessModuleAsync(ClaimsPrincipal user, string nomepack, CancellationToken cancellationToken = default);
}
