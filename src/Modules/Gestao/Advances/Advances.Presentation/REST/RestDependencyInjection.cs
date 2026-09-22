using Advances.Presentation.REST.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Advances.Presentation.REST;

/// <summary>
/// Registro de dependências da camada REST do módulo Advances
/// </summary>
public static class RestDependencyInjection
{
    public static IServiceCollection AddAdvancesRestPresentation(this IServiceCollection services)
    {
        services.AddScoped<IAdvancesLinkBuilder, AdvancesLinkBuilder>();
        return services;
    }
}
