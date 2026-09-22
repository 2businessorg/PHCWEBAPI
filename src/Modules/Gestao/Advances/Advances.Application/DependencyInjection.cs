using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Advances.Application;

/// <summary>
/// Registro de dependências da camada Application do módulo Advances
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAdvancesApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
