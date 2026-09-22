using Microsoft.Extensions.DependencyInjection;

namespace Providers.Presentation.REST;

public static class RestDependencyInjection
{
    public static IServiceCollection AddProvidersRestPresentation(this IServiceCollection services)
    {
        // Controllers são registados automaticamente via AddControllers no Host
        return services;
    }
}
