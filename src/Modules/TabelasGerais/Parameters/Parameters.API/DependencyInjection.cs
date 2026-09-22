using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Parameters.Infrastructure;
using Parameters.Application.Behaviors;

namespace Parameters.API;

/// <summary>
/// Extension methods para configurar o módulo de Parameters
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddParametersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ✅ MediatR com Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Features.CreateParameter.CreateParameterCommand).Assembly);

            // Pipeline Behaviors (ordem importa!)
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));      // 1. Logging
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));   // 2. Validação
            // cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>)); // 3. Performance (futuro)
            // cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));  // 4. Transação (futuro)
        });

        // ✅ FluentValidation
        services.AddValidatorsFromAssembly(
            typeof(Application.Features.CreateParameter.CreateParameterCommandValidator).Assembly);

        // Infrastructure Layer
        services.AddParametersInfrastructure(configuration);

        return services;
    }
}

