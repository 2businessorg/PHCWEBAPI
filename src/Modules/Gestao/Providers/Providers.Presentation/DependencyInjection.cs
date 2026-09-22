using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Providers.Application;
using Providers.Application.Behaviors;
using Providers.Infrastructure;
using Providers.Presentation.REST;

namespace Providers.Presentation;

/// <summary>
/// Extensão para adicionar o módulo Providers completo
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddProvidersPresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableRest = true,
        bool enableGraphQL = false)
    {
        // Application Layer (Mappers)
        services.AddProvidersApplication();

        // MediatR - Registrar handlers e behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Features.GetAllProviders.GetAllProvidersQuery).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(
            typeof(Application.Features.CreateProvider.CreateProviderCommand).Assembly);

        // Infrastructure Layer
        services.AddProvidersInfrastructure(configuration);

        // Presentation Layer - REST
        if (enableRest)
        {
            services.AddProvidersRestPresentation();
        }

        // GraphQL Future
        if (enableGraphQL)
        {
            // TODO: Implementar GraphQL se necessário
        }

        return services;
    }
}
