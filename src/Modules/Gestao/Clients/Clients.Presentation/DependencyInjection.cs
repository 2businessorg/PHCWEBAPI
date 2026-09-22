using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Clients.Application;
using Clients.Infrastructure;
using Clients.Presentation.REST;

namespace Clients.Presentation;

/// <summary>
/// Registro de dependências da camada Presentation
/// Entry point para registro do módulo completo
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddClientsPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Application Layer (MediatR, Validators, Mappers)
        services.AddClientsApplication();

        // Infrastructure Layer (DbContext, Repositories)
        services.AddClientsInfrastructure(configuration);

        // Presentation Layer - REST
        services.AddClientsRestPresentation();

        return services;
    }
}
