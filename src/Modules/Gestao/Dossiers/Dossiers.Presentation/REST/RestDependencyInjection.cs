using Dossiers.Presentation.REST.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dossiers.Presentation.REST;

/// <summary>
/// Registro de dependências REST API
/// </summary>
public static class RestDependencyInjection
{
    public static IServiceCollection AddDossiersRestPresentation(this IServiceCollection services)
    {
        services.AddScoped<IDossiersLinkBuilder, DossiersLinkBuilder>();
        return services;
    }
}
