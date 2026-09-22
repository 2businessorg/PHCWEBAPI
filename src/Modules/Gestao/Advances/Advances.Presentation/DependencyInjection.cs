using Advances.Application;
using Advances.Infrastructure;
using Advances.Presentation.REST;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Advances.Presentation;

/// <summary>
/// Ponto de entrada de registo de todas as camadas do módulo Advances
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAdvancesPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAdvancesApplication();
        services.AddAdvancesInfrastructure(configuration);
        services.AddAdvancesRestPresentation();

        return services;
    }
}
