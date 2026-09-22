using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace PHCAPI.Host.Filters;

/// <summary>
/// Bypass de autorização para testes locais.
/// Deve ser usado apenas em Development quando habilitado por configuração.
/// </summary>
public sealed class AllowAllAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
