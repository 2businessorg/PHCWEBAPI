using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Treasury.Application;
using Treasury.Infrastructure;
using Treasury.Presentation.REST;

namespace Treasury.Presentation;

/// <summary>
/// Entry point that registers all Treasury layers.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTreasuryPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTreasuryApplication();
        services.AddTreasuryInfrastructure(configuration);
        services.AddTreasuryRestPresentation();

        return services;
    }
}
