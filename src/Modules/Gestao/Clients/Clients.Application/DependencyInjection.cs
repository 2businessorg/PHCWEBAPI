using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;

namespace Clients.Application;

/// <summary>
/// Registro de dependências da camada Application
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddClientsApplication(this IServiceCollection services)
    {
        // MediatR (Commands & Queries)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation (Validators)
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
