using Microsoft.Extensions.DependencyInjection;
using Treasury.Presentation.REST.Services;

namespace Treasury.Presentation.REST;

/// <summary>
/// REST DI for the Treasury module.
/// </summary>
public static class RestDependencyInjection
{
    public static IServiceCollection AddTreasuryRestPresentation(this IServiceCollection services)
    {
        services.AddScoped<ITreasuryLinkBuilder, TreasuryLinkBuilder>();
        return services;
    }
}
