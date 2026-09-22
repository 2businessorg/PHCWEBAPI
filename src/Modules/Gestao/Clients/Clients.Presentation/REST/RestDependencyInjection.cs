using Microsoft.Extensions.DependencyInjection;
using Clients.Presentation.REST.Services;

namespace Clients.Presentation.REST;

/// <summary>
/// Registro de dependências REST API
/// </summary>
public static class RestDependencyInjection
{
    public static IServiceCollection AddClientsRestPresentation(this IServiceCollection services)
    {
        // Link builder para HATEOAS
        services.AddScoped<IClientsLinkBuilder, ClientsLinkBuilder>();

        // Controllers são registados automaticamente via AddControllers no Host
        return services;
    }
}
