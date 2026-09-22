using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Currencies.Application;
using Currencies.Infrastructure;

namespace Currencies.Presentation;

/// <summary>
/// Registro de dependências da camada Presentation do módulo Currencies.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCurrenciesPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCurrenciesApplication();
        services.AddCurrenciesInfrastructure(configuration);

        return services;
    }
}
