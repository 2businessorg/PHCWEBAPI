using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Parameters.Application;
using Parameters.Presentation.REST;
using Parameters.Presentation.GraphQL;
using Parameters.Application.Behaviors;
using Parameters.Infrastructure;

namespace Parameters.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddParametersPresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableRest = true,
        bool enableGraphQL = false)
    {
        // Application Layer (Mappers + MediatR + FluentValidation)
        services.AddParametersApplication();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Features.CreateParameter.CreateParameterCommand).Assembly);
           // cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(
            typeof(Application.Features.CreateParameter.CreateParameterCommandValidator).Assembly);

        // Infrastructure Layer
        services.AddParametersInfrastructure(configuration);

        // Presentation Layer (REST + GraphQL)
        if (enableRest)
        {
            services.AddParametersRestApi();
        }

        if (enableGraphQL)
        {
            services.AddParametersGraphQL();
        }

        return services;
    }
}
