using Dossiers.Application;
using Dossiers.Infrastructure;
using Dossiers.Presentation.REST;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dossiers.Presentation;

/// <summary>
/// Registro de dependências da camada Presentation
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDossiersPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDossiersApplication();
        services.AddDossiersInfrastructure(configuration);
        services.AddDossiersRestPresentation();

        return services;
    }
}
